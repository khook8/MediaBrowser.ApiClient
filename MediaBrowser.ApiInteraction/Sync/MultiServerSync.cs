﻿using MediaBrowser.ApiInteraction.Data;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.ApiInteraction.Sync
{
    public class MultiServerSync : IMultiServerSync
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly ILocalAssetManager _localAssetManager;
        private readonly IFileTransferManager _fileTransferManager;

        public MultiServerSync(IConnectionManager connectionManager, ILogger logger, ILocalAssetManager userActionAssetManager, IFileTransferManager fileTransferManager)
        {
            _connectionManager = connectionManager;
            _logger = logger;
            _localAssetManager = userActionAssetManager;
            _fileTransferManager = fileTransferManager;
        }

        public async Task Sync(IProgress<double> progress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var servers = await _connectionManager.GetAvailableServers(cancellationToken).ConfigureAwait(false);

            await Sync(servers, progress, cancellationToken).ConfigureAwait(false);
        }

        private async Task Sync(List<ServerInfo> servers, IProgress<double> progress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var numComplete = 0;
            double startingPercent = 0;
            double percentPerServer = 1;
            if (servers.Count > 0)
            {
                percentPerServer /= servers.Count;
            }

            _logger.Debug("Beginning MultiServerSync with {0} servers", servers.Count);

            foreach (var server in servers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentPercent = startingPercent;
                var serverProgress = new DoubleProgress();
                serverProgress.RegisterAction(pct =>
                {
                    var totalProgress = pct * percentPerServer;
                    totalProgress += currentPercent;
                    progress.Report(totalProgress);
                });

                // Grab the latest info from the connection manager about that server
                var serverInfo = await _connectionManager.GetServerInfo(server.Id).ConfigureAwait(false);

                if (serverInfo == null)
                {
                    serverInfo = server;
                }

                await new ServerSync(_connectionManager, _logger, _localAssetManager, _fileTransferManager, _connectionManager.ClientCapabilities)
                    .Sync(serverInfo, serverProgress, cancellationToken).ConfigureAwait(false);

                numComplete++;
                startingPercent = numComplete;
                startingPercent /= servers.Count;
                startingPercent *= 100;
                progress.Report(startingPercent);
            }

            progress.Report(100);
        }
    }
}
