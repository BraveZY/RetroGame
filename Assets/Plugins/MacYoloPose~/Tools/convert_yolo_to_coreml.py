#!/usr/bin/env python3
"""下载已锁定版本的 YOLOv8n-pose 权重并生成 macOS Core ML 姿态模型。"""

import argparse
import hashlib
import shutil
import subprocess
import tempfile
import urllib.request
from pathlib import Path

import coremltools as ct
import numpy as np
import torch
from ultralytics import YOLO

SOURCE_URL = "https://github.com/ultralytics/assets/releases/download/v8.4.0/yolov8n-pose.pt"
SOURCE_SHA256 = "c6fa93dd1ee4a2c18c900a45c1d864a1c6f7aba75d84f91648a30b7fb641d212"
INPUT_SHAPE = (1, 3, 320, 320)


def sha256(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    output = args.output.resolve()

    with tempfile.TemporaryDirectory(prefix="mac-yolo-coreml-") as temp_dir:
        temporary_root = Path(temp_dir)
        weights_path = temporary_root / "yolov8n-pose.pt"
        urllib.request.urlretrieve(SOURCE_URL, weights_path)
        if sha256(weights_path) != SOURCE_SHA256:
            raise RuntimeError("unexpected YOLOv8n-pose weights; refusing conversion")

        torch_model = YOLO(weights_path).model.fuse().eval()
        torch_model.model[-1].export = True
        sample = torch.zeros(INPUT_SHAPE)
        with torch.no_grad():
            traced_model = torch.jit.trace(torch_model, sample, check_trace=False)

        model = ct.convert(
            traced_model,
            convert_to="mlprogram",
            minimum_deployment_target=ct.target.macOS12,
            compute_precision=ct.precision.FLOAT16,
            inputs=[ct.TensorType(name="images", shape=INPUT_SHAPE, dtype=np.float32)],
            outputs=[ct.TensorType(name="output0")],
        )
        package = temporary_root / "YoloPose.mlpackage"
        model.save(package)

        prediction = ct.models.MLModel(str(package), compute_units=ct.ComputeUnit.ALL).predict(
            {"images": np.zeros(INPUT_SHAPE, dtype=np.float32)}
        )
        if tuple(prediction["output0"].shape) != (1, 56, 2100):
            raise RuntimeError("unexpected Core ML output contract")

        compiled_root = temporary_root / "compiled"
        subprocess.run(["xcrun", "coremlcompiler", "compile", str(package), str(compiled_root)], check=True)
        compiled_model = compiled_root / "YoloPose.mlmodelc"
        if output.exists():
            shutil.rmtree(output)
        shutil.copytree(compiled_model, output)


if __name__ == "__main__":
    main()
