/* 
*   NatDevice
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Devices {

    using System;

    /// <summary>
    /// Media device which provides media sample buffers.
    /// </summary>
    public interface IMediaDevice : IEquatable<IMediaDevice> {

        /// <summary>
        /// Device unique ID.
        /// </summary>
        string uniqueID { get; }

        /// <summary>
        /// Display friendly device name.
        /// </summary>
        string name { get; }

        /// <summary>
        /// Device location.
        /// </summary>
        DeviceLocation location { get; }

        /// <summary>
        /// Device is the default device for its media type.
        /// </summary>
        bool defaultForMediaType { get; }

        /// <summary>
        /// Is the device running?
        /// </summary>
        bool running { get; }

        /// <summary>
        /// Stop running.
        /// </summary>
        void StopRunning ();
    }
}