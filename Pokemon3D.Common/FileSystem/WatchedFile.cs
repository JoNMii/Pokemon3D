﻿using System;
using System.IO;
using System.Threading;

namespace Pokemon3D.Common.FileSystem
{
    /// <summary>
    /// A file watched by the <see cref="FileObserver"/> class.
    /// </summary>
    class WatchedFile : WatchedResource
    {
        DateTime _lastRead = DateTime.MinValue;
        event FileSystemEventHandler _watcherEvent;
        private int _handleCount = 0;

        public bool HasHandles
        {
            get { return _handleCount > 0; }
        }

        public WatchedFile(string filePath, FileSystemEventHandler eventHandler)
            : base(filePath)
        {
            string fileName = Path.GetFileName(ResourcePath);
            string directoryPath = Path.GetDirectoryName(ResourcePath);

            _watcher = new FileSystemWatcher(directoryPath, fileName);
            _watcher.Changed += OnWatcherEvent;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.EnableRaisingEvents = true;

            AddHandler(eventHandler);
        }

        private void OnWatcherEvent(object sender, FileSystemEventArgs e)
        {
            DateTime lastWriteTime = File.GetLastWriteTime(ResourcePath);

            if (lastWriteTime != _lastRead)
            {
                _lastRead = lastWriteTime;

                Thread.Sleep(300);

                _watcher.EnableRaisingEvents = false;
                _watcherEvent(this, e);
                _watcher.EnableRaisingEvents = true;
            }
        }

        public void AddHandler(FileSystemEventHandler eventHandler)
        {
            _watcherEvent += eventHandler;
            _handleCount++;
        }

        public void RemoveHandler(FileSystemEventHandler eventHandler)
        {
            _watcherEvent -= eventHandler;
            _handleCount--;
        }
    }
}
