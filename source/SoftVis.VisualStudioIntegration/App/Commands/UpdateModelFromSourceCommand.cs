﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Codartis.SoftVis.Util;

namespace Codartis.SoftVis.VisualStudioIntegration.App.Commands
{
    /// <summary>
    /// Updates the model (and consequently the diagram) to reflect the current state of the source code.
    /// Removes entities and relationships that no longer exist in code.
    /// </summary>
    internal sealed class UpdateModelFromSourceCommand : AsyncCommandBase
    {
        public UpdateModelFromSourceCommand(IAppServices appServices)
            : base(appServices)
        {
        }

        public override async Task ExecuteAsync()
        {
            await ShowProgressAndUpdateModelAsync();

            await HostUiServices.ShowDiagramWindowAsync();
            UiServices.ZoomToDiagram();
        }

        private async Task ShowProgressAndUpdateModelAsync()
        {
            using (var progressDialog = await HostUiServices.CreateProgressDialogAsync("Updating model entities:"))
            {
                progressDialog.ShowWithDelayAsync();

                try
                {
                    await UpdateModelAsync(progressDialog.CancellationToken, progressDialog.Progress);

                    progressDialog.Reset("Updating diagram nodes:", DiagramServices.Nodes.Count);

                    await UpdateDiagramAsync(progressDialog.CancellationToken, progressDialog.Progress);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private async Task UpdateModelAsync(CancellationToken cancellationToken, IIncrementalProgress progress)
        {
            await ModelServices.UpdateFromSourceAsync(cancellationToken, progress);
        }

        private async Task UpdateDiagramAsync(CancellationToken cancellationToken, IIncrementalProgress progress)
        {
            await DiagramServices.UpdateFromSourceAsync(cancellationToken, progress);
        }
    }
}