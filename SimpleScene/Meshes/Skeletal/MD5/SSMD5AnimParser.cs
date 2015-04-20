﻿using System;
using System.Text.RegularExpressions;
using OpenTK;

namespace SimpleScene
{
	// only used to register with the asset manager
	public class SSSkeletalAnimationMD5 : SSSkeletalAnimation
	{ 
		public SSSkeletalAnimationMD5 (int frameRate,
									   SSSkeletalJointBaseInfo[] jointInfo,
									   SSSkeletalJointLocation[][] frames,
									   SSAABB[] bounds)
			: base(frameRate, jointInfo, frames, bounds)
		{ }
	}

	public class SSMD5AnimParser : SSMD5Parser
	{
		public static SSSkeletalAnimationMD5 ReadAnimation(SSAssetManager.Context ctx, string filename)
		{
			var parser = new SSMD5AnimParser(ctx, filename);
			return parser.readAnimation();
		}

		private enum LocationFlags : byte {
			Tx = 1, Ty = 2, Tz = 4,
			Qx = 8, Qy = 16, Qz = 32
		}

		private SSMD5AnimParser (SSAssetManager.Context ctx, string filename)
			: base(ctx, filename)
		{
		}

		private SSSkeletalAnimationMD5 readAnimation()
		{
			Match[] matches;

			// header
			seekEntry ("MD5Version", "10");
			seekEntry ("commandline", SSMD5Parser.c_nameRegex);

			matches = seekEntry ("numFrames", SSMD5Parser.c_uintRegex);
			var numFrames = Convert.ToInt32 (matches [1].Value);

			matches = seekEntry ("numJoints", SSMD5Parser.c_uintRegex);
			var numJoints = Convert.ToInt32 (matches [1].Value);

			matches = seekEntry ("frameRate", SSMD5Parser.c_uintRegex);
			var frameRate = Convert.ToInt32 (matches [1].Value);

			matches = seekEntry ("numAnimatedComponents", SSMD5Parser.c_uintRegex);
			int numAnimatedComponents = Convert.ToInt32 (matches [1].Value);
			var floatComponents = new float[numAnimatedComponents];

			// hierarchy
			seekEntry ("hierarchy", "{");
			var hierarchy = new SSSkeletalJointBaseInfo[numJoints];
			var flags = new byte[numJoints];
			for (int j = 0; j < numJoints; ++j) {
				hierarchy [j] = readHierarchyEntry (out flags [j]);
			}
			seekEntry ("}");

			// bounds
			seekEntry ("bounds", "{");
			var bounds = new SSAABB[numFrames];
			for (int f = 0; f < numFrames; ++f) {
				bounds [f] = readBounds ();
			}
			seekEntry ("}");

			// base frame
			seekEntry ("baseframe", "{");
			for (int j = 0; j < numJoints; ++j) {
				hierarchy[j].BaseLocation = readBaseFrame ();
			}
			seekEntry ("}");

			// frames
			var frames = new SSSkeletalJointLocation[numFrames][];
			for (int f = 0; f < numFrames; ++f) {
				matches = seekEntry ("frame", SSMD5Parser.c_uintRegex, "{");
				int frameIdx = Convert.ToInt32 (matches [1].Value);
				frames [frameIdx] = readFrameJoints (flags, hierarchy, floatComponents);
				seekEntry ("}");
			}
			return new SSSkeletalAnimationMD5 (frameRate, hierarchy, frames, bounds);
		}

		private SSSkeletalJointBaseInfo readHierarchyEntry(out byte flags)
		{
			Match[] matches = seekEntry(
				SSMD5Parser.c_nameRegex, // name
				SSMD5Parser.c_intRegex, // parent index
				SSMD5Parser.c_uintRegex, // flags
				SSMD5Parser.c_uintRegex // start index (currently not used)
			);
			SSSkeletalJointBaseInfo ret = new SSSkeletalJointBaseInfo();
			ret.Name = matches[0].Value;
			ret.ParentIndex = Convert.ToInt32(matches[1].Value);
			flags = Convert.ToByte(matches[2].Value);
			//m_startIndex = Convert.ToInt32(matches[3].Value);
			return ret;
		}


		private SSAABB readBounds()
		{
			Match[] matches = seekEntry (
				SSMD5Parser.c_parOpen, 
					SSMD5Parser.c_floatRegex,
					SSMD5Parser.c_floatRegex,
					SSMD5Parser.c_floatRegex,
				SSMD5Parser.c_parClose,
				SSMD5Parser.c_parOpen,
					SSMD5Parser.c_floatRegex,
					SSMD5Parser.c_floatRegex,
					SSMD5Parser.c_floatRegex,
				SSMD5Parser.c_parClose
			);
			SSAABB ret;
			ret.Min.X = (float)Convert.ToDouble (matches [1].Value);
			ret.Min.Y = (float)Convert.ToDouble (matches [2].Value);
			ret.Min.Z = (float)Convert.ToDouble (matches [3].Value);
			ret.Max.X = (float)Convert.ToDouble (matches [6].Value);
			ret.Max.Y = (float)Convert.ToDouble (matches [7].Value);
			ret.Max.Z = (float)Convert.ToDouble (matches [8].Value);
			return ret;
		}

		private SSSkeletalJointLocation readBaseFrame()
		{
			Match[] matches = seekEntry (
				SSMD5Parser.c_parOpen, 
					SSMD5Parser.c_floatRegex,
					SSMD5Parser.c_floatRegex,
					SSMD5Parser.c_floatRegex,
				SSMD5Parser.c_parClose,
				SSMD5Parser.c_parOpen,
					SSMD5Parser.c_floatRegex,
					SSMD5Parser.c_floatRegex,
					SSMD5Parser.c_floatRegex,
					SSMD5Parser.c_parClose
			);
			SSSkeletalJointLocation loc;
			loc.Position.X = (float)Convert.ToDouble (matches [1].Value);
			loc.Position.Y = (float)Convert.ToDouble (matches [2].Value);
			loc.Position.Z = (float)Convert.ToDouble (matches [3].Value);
			loc.Orientation = new Quaternion ();
			loc.Orientation.X = (float)Convert.ToDouble (matches [6].Value);
			loc.Orientation.Y = (float)Convert.ToDouble (matches [7].Value);
			loc.Orientation.Z = (float)Convert.ToDouble (matches [8].Value);
			loc.ComputeQuatW ();
			return loc;
		}



		private SSSkeletalJointLocation[] readFrameJoints(byte[] jointFlags,
														  SSSkeletalJointBaseInfo[] jointInfos,
														  float[] floatComponents)
		{
			seekFloats (floatComponents);
			var thisFrameLocations = new SSSkeletalJointLocation[jointInfos.Length];
			int compIdx = 0;
			for (int j = 0; j < jointInfos.Length; ++j) {
				byte flags = jointFlags[j];
				SSSkeletalJointBaseInfo jointInfo = jointInfos [j];
				SSSkeletalJointLocation loc = jointInfo.BaseLocation;
				if ((flags & (byte)LocationFlags.Tx) != 0) {
					loc.Position.X = floatComponents [compIdx++];
				}
				if ((flags & (byte)LocationFlags.Ty) != 0) {
					loc.Position.Y = floatComponents [compIdx++];
				}
				if ((flags & (byte)LocationFlags.Tz) != 0) {
					loc.Position.Z = floatComponents [compIdx++];
				}
				if ((flags & (byte)LocationFlags.Qx) != 0) {
					loc.Orientation.X = floatComponents [compIdx++];
				}
				if ((flags & (byte)LocationFlags.Qy) != 0) {
					loc.Orientation.Y = floatComponents [compIdx++];
				}
				if ((flags & (byte)LocationFlags.Qz) != 0) {
					loc.Orientation.Z = floatComponents [compIdx++];
				}
				loc.ComputeQuatW ();

				if (jointInfo.ParentIndex >= 0) { // has a parent
					SSSkeletalJointLocation parentLoc = thisFrameLocations [jointInfo.ParentIndex];
					loc.Position = parentLoc.Position 
						+ Vector3.Transform (loc.Position, parentLoc.Orientation);
					loc.Orientation = Quaternion.Multiply (parentLoc.Orientation, 
						loc.Orientation);
					loc.Orientation.Normalize ();
				}
				thisFrameLocations[j] = loc;
			}
			return thisFrameLocations;
		}
	}
}

