from __future__ import annotations

import json
import shutil
from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
SOURCE = Path(
    "/Users/dukechen/.codex/generated_images/019eb99e-9125-74e0-9ab2-0d1764913fd9/"
    "ig_015d46c6f04078b1016a2d166103048195a26c3dbba811e0ae.png"
)
OUT = ROOT / "prototype/public/assets/ui_time_play_home"


SLICES = [
    {
        "name": "reference_master",
        "group": "source",
        "box": [0, 0, 1672, 941],
        "use": "完整 image2 母版，只作风格和对位参考。",
        "owner": "reference",
        "alpha": "none",
    },
    {
        "name": "bg_time_play_hall_full",
        "group": "slices/background",
        "box": [0, 0, 1672, 941],
        "use": "整屏大厅背景草案，仍含前景 UI，需要 PS 清理成干净底图。",
        "owner": "raster",
        "alpha": "none",
    },
    {
        "name": "entry_childhood_full_raw",
        "group": "slices/entries/raw",
        "box": [146, 170, 455, 535],
        "use": "童年操场入口整块，对位用。",
        "owner": "raster",
        "alpha": "none",
    },
    {
        "name": "entry_arcade_full_raw",
        "group": "slices/entries/raw",
        "box": [607, 151, 454, 575],
        "use": "客厅街机入口整块，对位用。",
        "owner": "raster",
        "alpha": "none",
    },
    {
        "name": "entry_tv_fitness_full_raw",
        "group": "slices/entries/raw",
        "box": [1055, 165, 436, 555],
        "use": "电视健美操入口整块，对位用。",
        "owner": "raster",
        "alpha": "none",
    },
    {
        "name": "scene_childhood_playground_raw",
        "group": "slices/entries/scenes",
        "box": [190, 299, 353, 395],
        "use": "童年操场入口内景，建议后续作为主题封面重绘或精修。",
        "owner": "raster",
        "alpha": "none",
    },
    {
        "name": "scene_arcade_raw",
        "group": "slices/entries/scenes",
        "box": [665, 284, 337, 395],
        "use": "客厅街机入口内景，建议后续作为主题封面重绘或精修。",
        "owner": "raster",
        "alpha": "none",
    },
    {
        "name": "scene_tv_fitness_raw",
        "group": "slices/entries/scenes",
        "box": [1120, 334, 335, 318],
        "use": "电视健美操入口内景，建议后续作为主题封面重绘或精修。",
        "owner": "raster",
        "alpha": "none",
    },
    {
        "name": "panel_theme_info_raw",
        "group": "slices/panels/raw",
        "box": [187, 746, 955, 145],
        "use": "底部主题信息面板原始裁切，文字需重建。",
        "owner": "raster+text-rebuild",
        "alpha": "none",
        "nineSlice": {"border": [42, 34, 42, 34]},
    },
    {
        "name": "panel_theme_info_masked",
        "group": "slices/panels/masked",
        "box": [187, 746, 955, 145],
        "use": "底部主题信息面板透明草案，可临时进原型；文字需重建。",
        "owner": "raster+text-rebuild",
        "alpha": "rounded-rect-mask",
        "radius": 34,
        "nineSlice": {"border": [42, 34, 42, 34]},
    },
    {
        "name": "button_enter_orange_raw",
        "group": "slices/buttons/raw",
        "box": [878, 786, 265, 78],
        "use": "进入按钮原始裁切，文字和三角箭头建议重建/分离。",
        "owner": "raster+text-rebuild",
        "alpha": "none",
        "nineSlice": {"border": [36, 24, 36, 24]},
    },
    {
        "name": "button_enter_orange_masked",
        "group": "slices/buttons/masked",
        "box": [878, 786, 265, 78],
        "use": "进入按钮透明草案，可临时进原型；正式版需 PS 去字和做四态。",
        "owner": "raster+text-rebuild",
        "alpha": "rounded-rect-mask",
        "radius": 38,
        "nineSlice": {"border": [36, 24, 36, 24]},
    },
    {
        "name": "panel_daily_energy_raw",
        "group": "slices/panels/raw",
        "box": [1215, 790, 171, 104],
        "use": "今日活力卡原始裁切，数字用文本重建。",
        "owner": "raster+text-rebuild",
        "alpha": "none",
        "nineSlice": {"border": [24, 22, 24, 22]},
    },
    {
        "name": "panel_daily_energy_masked",
        "group": "slices/panels/masked",
        "box": [1215, 790, 171, 104],
        "use": "今日活力卡透明草案。",
        "owner": "raster+text-rebuild",
        "alpha": "rounded-rect-mask",
        "radius": 24,
        "nineSlice": {"border": [24, 22, 24, 22]},
    },
    {
        "name": "panel_achievement_raw",
        "group": "slices/panels/raw",
        "box": [1403, 790, 244, 104],
        "use": "最新成就卡原始裁切，成就名用文本重建。",
        "owner": "raster+text-rebuild",
        "alpha": "none",
        "nineSlice": {"border": [24, 22, 24, 22]},
    },
    {
        "name": "panel_achievement_masked",
        "group": "slices/panels/masked",
        "box": [1403, 790, 244, 104],
        "use": "最新成就卡透明草案。",
        "owner": "raster+text-rebuild",
        "alpha": "rounded-rect-mask",
        "radius": 24,
        "nineSlice": {"border": [24, 22, 24, 22]},
    },
    {
        "name": "avatar_child_circle",
        "group": "slices/icons/masked",
        "box": [1428, 31, 96, 96],
        "use": "右上玩家头像圆形草案，正式版建议拆头像框和头像图。",
        "owner": "raster",
        "alpha": "ellipse-mask",
    },
    {
        "name": "button_settings_circle",
        "group": "slices/icons/masked",
        "box": [1535, 45, 82, 82],
        "use": "设置按钮圆形草案，正式版建议拆底和齿轮 icon。",
        "owner": "raster",
        "alpha": "ellipse-mask",
    },
    {
        "name": "icon_theme_star",
        "group": "slices/icons/raw",
        "box": [184, 735, 78, 82],
        "use": "底部主题星星徽章原始裁切，需 PS 精抠发光边。",
        "owner": "raster",
        "alpha": "none",
    },
    {
        "name": "coin_decoration_raw",
        "group": "slices/decorations/raw",
        "box": [939, 646, 58, 66],
        "use": "金币装饰原始裁切，需 PS 精抠。",
        "owner": "raster",
        "alpha": "none",
    },
]


TEXT_REBUILD = [
    "时光体感馆",
    "TIME PLAY",
    "今天想回到哪一段童年？",
    "童年操场",
    "客厅街机",
    "电视健美操",
    "铃声一响，跑向操场。",
    "木头人、跳房子、丢手绢都在这里开始。",
    "进入童年操场",
    "今日活力",
    "68/100",
    "最新成就",
    "跳房子高手",
]


def crop_box(image: Image.Image, item: dict) -> Image.Image:
    x, y, w, h = item["box"]
    return image.crop((x, y, x + w, y + h))


def apply_mask(image: Image.Image, item: dict) -> Image.Image:
    mode = item.get("alpha")
    if mode not in {"rounded-rect-mask", "ellipse-mask"}:
        return image

    rgba = image.convert("RGBA")
    mask = Image.new("L", rgba.size, 0)
    draw = ImageDraw.Draw(mask)
    if mode == "ellipse-mask":
        draw.ellipse((0, 0, rgba.width - 1, rgba.height - 1), fill=255)
    else:
        radius = int(item.get("radius", 20))
        draw.rounded_rectangle((0, 0, rgba.width - 1, rgba.height - 1), radius=radius, fill=255)
    rgba.putalpha(mask)
    return rgba


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / "source").mkdir(parents=True, exist_ok=True)
    shutil.copy2(SOURCE, OUT / "source/master.png")

    image = Image.open(SOURCE).convert("RGB")
    manifest = {
        "source": str(SOURCE),
        "sourceSize": {"width": image.width, "height": image.height},
        "alphaSource": False,
        "coordinateSpace": "top-left pixels in 1672x941 master",
        "unity": {
            "pixelsPerUnit": 100,
            "defaultPivot": [0.5, 0.5],
            "atlas": "ui_time_play_home",
        },
        "slices": [],
        "textRebuild": TEXT_REBUILD,
    }

    for item in SLICES:
        target_dir = OUT / item["group"]
        target_dir.mkdir(parents=True, exist_ok=True)
        cropped = crop_box(image, item)
        cropped = apply_mask(cropped, item)
        path = target_dir / f"{item['name']}.png"
        cropped.save(path)

        x, y, w, h = item["box"]
        manifest["slices"].append(
            {
                "name": item["name"],
                "path": str(path.relative_to(OUT)),
                "group": item["group"],
                "sourceRect": {"x": x, "y": y, "width": w, "height": h},
                "size": {"width": w, "height": h},
                "use": item["use"],
                "owner": item["owner"],
                "alpha": item["alpha"],
                "pivot": [0.5, 0.5],
                "atlasReady": item["alpha"] != "none",
                "nineSlice": item.get("nineSlice"),
            }
        )

    report = {
        "assetName": "ui_time_play_home",
        "status": "draft-slicing",
        "summary": "第一版工程拆分：保留母版、原坐标粗拆、部分圆角/圆形透明草案。",
        "source": manifest["source"],
        "sourceSize": manifest["sourceSize"],
        "alphaEvidence": {
            "sourceHasAlpha": False,
            "method": "raw crop + procedural masks only",
            "mattePath": None,
            "note": "源图为 RGB PNG。复杂发光、人物、标题牌、金币和星星未做正式 AI matte，需要 Photoshop 或 BiRefNet 精抠。",
        },
        "rebuildInsteadOfSlice": {
            "text": TEXT_REBUILD,
            "hitAreas": [
                "三个主题入口按钮热区",
                "进入主题主按钮热区",
                "右上设置按钮热区",
                "右下 HUD 卡片热区",
            ],
        },
        "photoshopNextPass": [
            "清理 bg_time_play_hall_full 中的前景 UI，导出干净背景。",
            "把三个入口门框与内景拆成 frame / scene / selected glow / title plate。",
            "去掉按钮和面板中的烙印文字，补 normal / pressed / disabled / selected 状态。",
            "对 icon_theme_star、coin_decoration_raw、门框星光做边缘精抠。",
        ],
        "photoshopMcpStatus": {
            "availableToolsObserved": [
                "create_document",
                "export_png",
                "run_action",
                "remove_background_ai",
                "matte_active_source",
                "upscale_active_source",
                "run_production_ui_workflow",
            ],
            "mattingAttempted": [
                "icon_theme_star",
                "coin_decoration_raw",
                "button_settings_circle",
            ],
            "blockedBy": "TOOL_GATEWAY_UNAUTHORIZED: Invalid or missing Photoshop MCP gateway token.",
            "note": "PS MCP 抠图能力存在，但当前 Codex 工具网关缺少授权 token；本次只交付 draft slicing 与程序 mask，不声明 BiRefNet 抠图完成。",
        },
        "unityImportNotes": [
            "masked 面板和按钮可先作为 Sprite 测试，但正式版需替换为无文字底图。",
            "panel_theme_info、button_enter_orange、panel_daily_energy、panel_achievement 已给出 9-slice border 建议。",
            "所有中文、数字、状态文字使用 TMP/Unity Text 或 HTML 文本重建。",
            "raw 裁切用于对位和 PS 精修，不建议直接进 atlas。",
        ],
        "remainingRisks": [
            "入口门框和背景强粘连，自动 mask 会损坏发光边。",
            "部分按钮/面板仍含文字，不能作为最终本地化资源。",
            "干净背景需要人工或生成式修补，当前只提供带 UI 的背景参考。",
        ],
    }

    (OUT / "unity-sprites").mkdir(parents=True, exist_ok=True)
    (OUT / "unity-sprites/manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    (OUT / "slicing-report.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8"
    )

    print(f"Wrote {len(SLICES)} slices to {OUT}")


if __name__ == "__main__":
    main()
