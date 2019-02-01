// Accord Direct Show Library
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright � C�sar Souza, 2009-2017
// cesarsouza at gmail.com
//
// AForge Direct Show Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright � AForge.NET, 2009-2013
// contacts@aforgenet.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace Accord.Video.DirectShow
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;

    using Accord.Video;
    using Accord.Video.DirectShow.Internals;
    using System.Linq;

    /// <summary>
    /// Capabilities of video device such as frame size and frame rate.
    /// </summary>
    public class VideoCapabilities
    {
        /// <summary>
        /// Frame size supported by video device.
        /// </summary>
        public readonly Size FrameSize;

        /// <summary>
        /// Frame rate supported by video device for corresponding <see cref="FrameSize">frame size</see>.
        /// </summary>
        /// 
        /// <remarks><para><note>This field is depricated - should not be used.
        /// Its value equals to <see cref="AverageFrameRate"/>.</note></para>
        /// </remarks>
        /// 
        [Obsolete("No longer supported. Use AverageFrameRate instead.")]
        public int FrameRate
        {
            get { return AverageFrameRate; }
        }

        /// <summary>
        /// Average frame rate of video device for corresponding <see cref="FrameSize">frame size</see>.
        /// </summary>
        public readonly int AverageFrameRate;

        /// <summary>
        /// Maximum frame rate of video device for corresponding <see cref="FrameSize">frame size</see>.
        /// </summary>
        public readonly int MaximumFrameRate;

        /// <summary>
        /// Number of bits per pixel provided by the camera.
        /// </summary>
        public readonly int BitCount;

        public readonly AMMediaType MediaType;

        internal VideoCapabilities()
        {
        }

        // Retrieve capabilities of a video device
        static internal VideoCapabilities[] FromStreamConfig(IAMStreamConfig videoStreamConfig)
        {
            if (videoStreamConfig == null)
                throw new ArgumentNullException("videoStreamConfig");

            // ensure this device reports capabilities
            int count, size;
            int hr = videoStreamConfig.GetNumberOfCapabilities(out count, out size);

            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);

            if (count <= 0)
                throw new NotSupportedException("This video device does not report capabilities.");

            if (size > Marshal.SizeOf(typeof(VideoStreamConfigCaps)))
                throw new NotSupportedException("Unable to retrieve video device capabilities. This video device requires a larger VideoStreamConfigCaps structure.");

            var caps = new List<VideoCapabilities>();

            for (int i = 0; i < count; i++)
            {
                try
                {
                    caps.Add(new VideoCapabilities(videoStreamConfig, i));
                }
                catch
                {
                }
            }

            return caps.ToArray();
        }

        // Retrieve capabilities of a video device
        internal VideoCapabilities(IAMStreamConfig videoStreamConfig, int index)
        {
            AMMediaType mediaType = null;
            VideoStreamConfigCaps caps = new VideoStreamConfigCaps();

            try
            {
                // retrieve capabilities struct at the specified index
                int hr = videoStreamConfig.GetStreamCaps(index, out mediaType, caps);

                if (hr != 0)
                    Marshal.ThrowExceptionForHR(hr);

                if (mediaType.FormatType == FormatType.VideoInfo)
                {
                    VideoInfoHeader videoInfo = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));

                    FrameSize = new Size(videoInfo.BmiHeader.Width, videoInfo.BmiHeader.Height);
                    BitCount = videoInfo.BmiHeader.BitCount;
                    AverageFrameRate = (int)(10000000 / videoInfo.AverageTimePerFrame);
                    MaximumFrameRate = (int)(10000000 / caps.MinFrameInterval);
                    MediaType = mediaType;
                }
                else if (mediaType.FormatType == FormatType.VideoInfo2)
                {
                    VideoInfoHeader2 videoInfo = (VideoInfoHeader2)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader2));

                    FrameSize = new Size(videoInfo.BmiHeader.Width, videoInfo.BmiHeader.Height);
                    BitCount = videoInfo.BmiHeader.BitCount;
                    AverageFrameRate = (int)(10000000 / videoInfo.AverageTimePerFrame);
                    MaximumFrameRate = (int)(10000000 / caps.MinFrameInterval);
                    MediaType = mediaType;
                }
                else
                {
                    throw new ApplicationException("Unsupported format found.");
                }

                // ignore 12 bpp formats for now, since it was noticed they cause issues on Windows 8
                // TODO: proper fix needs to be done so ICaptureGraphBuilder2::RenderStream() does not fail
                // on such formats
                if (BitCount <= 12)
                    throw new ApplicationException("Unsupported format found.");
            }
            finally
            {
                //if (mediaType != null)
                //    mediaType.Dispose();
            }
        }

        /// <summary>
        /// Check if the video capability equals to the specified object.
        /// </summary>
        /// 
        /// <param name="obj">Object to compare with.</param>
        /// 
        /// <returns>Returns true if both are equal are equal or false otherwise.</returns>
        /// 
        public override bool Equals(object obj)
        {
            return Equals(obj as VideoCapabilities);
        }

        /// <summary>
        /// Check if two video capabilities are equal.
        /// </summary>
        /// 
        /// <param name="vc2">Second video capability to compare with.</param>
        /// 
        /// <returns>Returns true if both video capabilities are equal or false otherwise.</returns>
        /// 
        public bool Equals(VideoCapabilities vc2)
        {
            if (vc2 is null)
                return false;

            return FrameSize == vc2.FrameSize && BitCount == vc2.BitCount && MediaType == vc2.MediaType;
        }

        /// <summary>
        /// Get hash code of the object.
        /// </summary>
        /// 
        /// <returns>Returns hash code ot the object </returns>
        public override int GetHashCode()
        {
            return FrameSize.GetHashCode() ^ BitCount ^ MediaType.GetHashCode();
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// 
        /// <param name="a">First object to check.</param>
        /// <param name="b">Seconds object to check.</param>
        /// 
        /// <returns>Return true if both objects are equal or false otherwise.</returns>
        public static bool operator ==(VideoCapabilities a, VideoCapabilities b)
        {
            // if both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;

            // if one is null, but not both, return false.
            if ((a is null) || (b is null))
                return false;

            return a.Equals(b);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// 
        /// <param name="a">First object to check.</param>
        /// <param name="b">Seconds object to check.</param>
        /// 
        /// <returns>Return true if both objects are not equal or false otherwise.</returns>
        public static bool operator !=(VideoCapabilities a, VideoCapabilities b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return String.Format("{0}x{1}, {2} fps ({3} max fps), {4} bpp",
                FrameSize.Width, FrameSize.Height,
                AverageFrameRate, MaximumFrameRate,
                BitCount);
        }
    }
}
