﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Codartis.SoftVis.Diagramming;
using Codartis.SoftVis.Service;
using Codartis.SoftVis.Service.Plugins;
using Codartis.SoftVis.TestHostApp.Diagramming;
using Codartis.SoftVis.TestHostApp.Modeling;
using Codartis.SoftVis.TestHostApp.TestData;
using Codartis.SoftVis.TestHostApp.UI;
using Codartis.SoftVis.UI;
using Codartis.SoftVis.UI.Wpf.View;
using Codartis.SoftVis.UI.Wpf.ViewModel;
using Codartis.SoftVis.Util;
using Codartis.SoftVis.Util.UI.Wpf.Commands;
using Codartis.SoftVis.Util.UI.Wpf.Dialogs;
using Codartis.SoftVis.Util.UI.Wpf.Resources;
using Codartis.SoftVis.Util.UI.Wpf.ViewModels;

namespace Codartis.SoftVis.TestHostApp
{
    class MainWindowViewModel : ViewModelBase
    {
        private const string DiagramStylesXaml = "Resources/Styles.xaml";

        private readonly ResourceDictionary _resourceDictionary;

        private readonly IVisualizationService _visualizationService;
        private readonly TestModelStore _testModelStore;
        private readonly DiagramId _diagramId;

        private int _modelItemGroupIndex;
        private int _nextToRemoveModelItemGroupIndex;
        private double _selectedDpi;

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand ZoomToContentCommand { get; }
        public ICommand CopyToClipboardCommand { get; }

        public MainWindow Window { get; set; }
        public IDiagramUi DiagramUi { get; set; }
        public IDiagramStlyeProvider DiagramStlyeProvider { get; set; }

        public MainWindowViewModel()
        {
            _resourceDictionary = ResourceHelpers.GetResourceDictionary(DiagramStylesXaml, Assembly.GetExecutingAssembly());

            AddCommand = new DelegateCommand(AddShapes);
            RemoveCommand = new DelegateCommand(RemoveShapes);
            ZoomToContentCommand = new DelegateCommand(ZoomToContent);
            CopyToClipboardCommand = new DelegateCommand(CopyToClipboardAsync);

            SelectedDpi = 300;

            _visualizationService = new VisualizationService(
                new TestModelStoreFactory(),
                new TestDiagramStoreFactory(),
                new TestDiagramShapeFactory(),
                new DiagramUiFactory(),
                new TestDiagramShapeUiFactory(),
                new DiagramPluginFactory(new TestLayoutPriorityProvider(), new TestDiagramShapeFactory()),
                new[]
                {
                    DiagramPluginId.AutoLayoutDiagramPlugin,
                    DiagramPluginId.ConnectorHandlerDiagramPlugin,
                    DiagramPluginId.ModelTrackingDiagramPlugin
                }
            );
            _visualizationService.ModelNodeInvoked += modelNode => Debug.WriteLine($"ModelNodeInvoked: {modelNode}");

            _diagramId = _visualizationService.CreateDiagram(minZoom: 0.2, maxZoom: 5, initialZoom: 1);
            DiagramUi = _visualizationService.GetDiagramUi(_diagramId);

            _testModelStore = (TestModelStore)_visualizationService.GetModelStore();
            TestModelCreator.Create(_testModelStore);
            //BigTestModelCreator.Create(_testModelStore, 2, 5);
        }

        public double SelectedDpi
        {
            get { return _selectedDpi; }
            set
            {
                _selectedDpi = value;
                OnPropertyChanged();
            }
        }

        private void AddShapes()
        {
            if (_modelItemGroupIndex == _testModelStore.CurrentTestModel.ItemGroups.Count)
                return;

            var modelNodes = _testModelStore.CurrentTestModel.ItemGroups[_modelItemGroupIndex];

            foreach (var modelNode in modelNodes)
                _visualizationService.ShowModelNode(_diagramId, modelNode);

            _modelItemGroupIndex++;

            //_testDiagram.Save(@"c:\big.xml");

            ZoomToContent();
        }

        private void RemoveShapes()
        {
            if (_nextToRemoveModelItemGroupIndex == _testModelStore.CurrentTestModel.ItemGroups.Count)
                return;

            var modelNodes = _testModelStore.CurrentTestModel.ItemGroups[_nextToRemoveModelItemGroupIndex];

            foreach (var modelNode in modelNodes)
                _visualizationService.HideModelNode(_diagramId, modelNode);

            _nextToRemoveModelItemGroupIndex++;

            ZoomToContent();
        }

        private void ZoomToContent()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            timer.Tick += (s, o) =>
            {
                timer.Stop();
                _visualizationService.ZoomToContent(_diagramId);
            };
            timer.Start();
        }

        private async void CopyToClipboardAsync()
        {
            _visualizationService.InitializeUi(DiagramStlyeProvider, _resourceDictionary);

            try
            {
                using (var progressDialog = new ProgressDialog(Window, "TestHostApp", "Generating image.."))
                {
                    progressDialog.ShowWithDelayAsync();

                    var bitmapSource = await _visualizationService.CreateDiagramImageAsync(_diagramId, SelectedDpi, 10,
                        progressDialog.CancellationToken, progressDialog.Progress, progressDialog.MaxProgress);

                    progressDialog.Reset("Copying image to clipboard...", showProgressNumber: false);

                    // Clipboard operations must run on STA thread.
                    await Task.Factory.StartSTA(() => Clipboard.SetImage(bitmapSource), progressDialog.CancellationToken);
                }
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("Cannot export the image because it is too large. Please select a smaller DPI value.", "TestHostApp");
            }
        }
    }
}
