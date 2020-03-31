﻿using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;

namespace Qsor.Beatmaps
{
    public class WorkingBeatmap : Beatmap
    {
        public Track Track;
        public Texture Background;
        
        [BackgroundDependencyLoader]
        private void Load()
        {
            var audioFileStream = BeatmapStorage.GetStream(General.AudioFilename);
            
            Track = new TrackBass(audioFileStream);
            Background = Texture.FromStream(BeatmapStorage.GetStream(BackgroundFilename));
        }
    }
}