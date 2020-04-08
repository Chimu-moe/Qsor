﻿using System;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using Qsor.Game.Beatmaps;
using Qsor.Game.Configuration;
using Qsor.Game.Database;
using Qsor.Game.Online;
using Qsor.Game.Overlays;
using Qsor.Game.Updater;
using Qsor.Game.Utils;

namespace Qsor.Game
{
    public class QsorBaseGame : osu.Framework.Game
    {
        protected readonly string[] Args;
        protected BeatmapManager BeatmapManager;
        protected UserManager UserManager;
        
        protected BeatmapMirrorAccess BeatmapMirrorAccess;
        
        protected QsorDbContextFactory QsorDbContextFactory;
        protected QsorConfigManager ConfigManager;
        
        protected NotificationOverlay NotificationOverlay;
        protected UpdaterOverlay UpdaterOverlay;
        protected SentryLogger SentryLogger;

        public Updater.Updater Updater;
        
        private DependencyContainer _dependencies;

        public string Version => Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? string.Empty;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            _dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public QsorBaseGame(string[] args)
        {
            Args = args;
        }
        
        [BackgroundDependencyLoader]
        private void Load(Storage storage)
        {
            _dependencies.CacheAs(BeatmapManager = new BeatmapManager());
            _dependencies.Cache(UserManager = new UserManager());
            
            _dependencies.Cache(BeatmapMirrorAccess = new BeatmapMirrorAccess());
            
            _dependencies.Cache(QsorDbContextFactory = new QsorDbContextFactory(storage));
            _dependencies.Cache(ConfigManager = new QsorConfigManager(storage));
            
            _dependencies.Cache(NotificationOverlay = new NotificationOverlay());
            _dependencies.Cache(SentryLogger = new SentryLogger(this));
            
            _dependencies.CacheAs(this);
            _dependencies.CacheAs(Host);
            
            Resources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(typeof(QsorGame).Assembly), @"Resources"));
            
            QsorDbContextFactory.Get().Migrate();
            
            AddInternal(BeatmapManager);
            AddInternal(NotificationOverlay);

            if (Updater == null)
                Updater = new DummyUpdater();
            
            UpdaterOverlay = new UpdaterOverlay();
                
            _dependencies.Cache(UpdaterOverlay);
            _dependencies.CacheAs(Updater);
                
            LoadComponent(Updater);

            Scheduler.AddDelayed(() => throw new NotImplementedException("Test Exception"), 5000);
            
            ConfigManager.Save();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing) {
                ConfigManager?.Dispose();
                BeatmapManager?.Dispose();
                NotificationOverlay?.Dispose();
                
                SentryLogger?.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}