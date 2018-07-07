using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ArcGISmap.ViewModels
{
    public class MapViewModel : BindableBase
    {
        /// <summary>
        /// CONSTANT DATA
        /// </summary>
        private const int N = 5;
        private const string ADDRESS_IMG_FOLDER = @"D:\icons";
        private const string FILENAME_POINTSDATA = "pointsData.json";
        private const string FILENAME_POLYGONSDATA = "polygonsData.json";

        /// <summary>
        /// Info padnel's variable to display info about action, currrent status and other related datas
        /// </summary>
        private string _InfoText;
        public string InfoText { set { this.SetProperty(ref _InfoText, value); } get => _InfoText; }

        /// <summary>
        /// public variables 
        /// </summary>
        public Map MyMap { set; get; }
        public DelegateCommand MapViewTapped { set; get; }
        public DelegateCommand PointBtnClick { set; get; }
        public DelegateCommand PolygonBtnClick { set; get; }
        public DelegateCommand SelectByRadiusBtnClick { set; get; }
        public DelegateCommand SaveMapData { set; get; }
        public DelegateCommand ClearCurrentCommandBtnClick { set; get; }
        public DelegateCommand Undo { set; get; }
        public DelegateCommand Redo { set; get; }

        private int currentBtnAction = 0;
        private MapView MyMapView;
        private FeatureCollection pointsData = new FeatureCollection();
        private FeatureCollection polygonsData = new FeatureCollection();

        /// <summary>
        /// Load images file addresses to imgList
        /// </summary>
        private List<string> imgList = new List<string>();
        private void LoadImgAddress()
        {
            var files = System.IO.Directory.GetFiles(ADDRESS_IMG_FOLDER, "*.png");
            for (int i = 0; i < N; i++)
            {
                imgList.Add(files[i]);
            }
        }
        
        /// <summary>
        /// Inits and loads Map Data
        /// </summary>
        private void LoadMapData()
        {
            InfoText = "Loading map...";
            LoadImgAddress();
            if (File.Exists(FILENAME_POINTSDATA))
            {
                pointsData = FeatureCollection.FromJson(File.ReadAllText(FILENAME_POINTSDATA));
            }
            else
            {
                for (int i = 0; i < N; i++)
                {
                    pointsData.Tables.Add(new FeatureCollectionTable(new Field[] { new Field(FieldType.Text, "Text", null, 50) }, GeometryType.Point, SpatialReferences.WebMercator));
                }

            }
            int j = 0;
            foreach (var table in pointsData.Tables)
            {
                table.Renderer = new SimpleRenderer(new PictureMarkerSymbol(new Uri(imgList.ElementAt(j++))));
            }



            if (File.Exists(FILENAME_POLYGONSDATA))
            {
                polygonsData = FeatureCollection.FromJson(File.ReadAllText(FILENAME_POLYGONSDATA));
            }
            else
            {
                for (int i = 0; i < N; i++)
                {
                    polygonsData.Tables.Add(new FeatureCollectionTable(new Field[] { new Field(FieldType.Text, "Text", null, 50) }, GeometryType.Polygon, SpatialReferences.WebMercator));
                }

            }


            int kx, ky;
            Random rand = new Random();
            foreach (var table in polygonsData.Tables)
            {

                kx = rand.Next();
                ky = rand.Next();
                byte a = Convert.ToByte(100);
                byte r = Convert.ToByte(Math.Abs(2 * kx - ky) % 255);
                byte g = Convert.ToByte(Math.Abs(kx - 3 * ky) % 255);
                byte b = Convert.ToByte(Math.Abs(kx + ky) % 255);

                //var outlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Blue, 1.0);
                table.Renderer = new SimpleRenderer(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(a, r, g, b), null));
            }
            //polygonsData.Tables.ElementAt(0).Renderer = new SimpleRenderer(new PictureFillSymbol(new Uri(@"D:\img\home.jpg")));
            polygonsData.Tables.ElementAt(0).Renderer = new SimpleRenderer(new PictureFillSymbol(new Uri(imgList.ElementAt(N-1))));

            MyMap.OperationalLayers.Add(new FeatureCollectionLayer(polygonsData));
            MyMap.OperationalLayers.Add(new FeatureCollectionLayer(pointsData));
            //foreach (var table in pointsData.Tables)
            //{
            //    table.FeatureLayer.MinScale = 1000000;
            //}
            InfoText = "Loading map is completed";
        }

        private List<MapPoint> tempMapPoints = new List<MapPoint>();
        private PolygonBuilder polygonBuilder = new PolygonBuilder(SpatialReferences.WebMercator);
        private GraphicsOverlay tempPointGraphicsOverlay = new GraphicsOverlay();
        private GraphicsOverlay tempPolygonGraphicsOverlay = new GraphicsOverlay();

        public MapViewModel(MapView _MapView)
        {

            MyMapView = _MapView;
            MyMap = new Map(SpatialReferences.WebMercator);
            ////string str = "sdsd";
            ////InfoText = str;
            LoadMapData();

            PointBtnClick = new DelegateCommand(() =>
            {
                currentBtnAction = 1;
                MyMapView.GraphicsOverlays.Clear();
                InfoText = "Draw POINT button clicked";
            });

            PolygonBtnClick = new DelegateCommand(() =>
            {   
                currentBtnAction = 2;
                MyMapView.GraphicsOverlays.Clear();
                MyMapView.GraphicsOverlays.Add(tempPointGraphicsOverlay);
                MyMapView.GraphicsOverlays.Add(tempPolygonGraphicsOverlay);
                InfoText = "Draw POLYGON button clicked";
            });

            SelectByRadiusBtnClick = new DelegateCommand(() =>
            {   
                currentBtnAction = 3;
                MyMapView.GraphicsOverlays.Clear();
                InfoText = "SELECT BY RADIUS button clicked";
            });

            SaveMapData = new DelegateCommand(() =>
            {
                InfoText = "Saving Map...";
                string poinsJson = pointsData.ToJson();
                File.WriteAllText("pointsData.json", poinsJson);

                string polygonsJson = polygonsData.ToJson();
                File.WriteAllText("polygonsData.json", polygonsJson);
                InfoText = "Map Saved";
            });

            ClearCurrentCommandBtnClick = new DelegateCommand(() =>
            {
                InfoText = "";
                currentBtnAction = 0;
                MyMapView.GraphicsOverlays.Clear();
            });

            
            Undo = new DelegateCommand(() =>
            {
                switch (currentBtnAction)
                {
                    case 1:
                        UndoPointAction();
                        break;

                    case 2:
                        UndoPolygonAction();
                        break;

                    case 3:
                        //SelectByRadius(e);
                        break;

                    default:
                        break;

                }

            });


            Redo = new DelegateCommand(() =>
            {

                switch (currentBtnAction)
                {
                    case 1:
                        RedoPointAction();
                        break;

                    case 2:
                        RedoPolygonAction();
                        break;

                    case 3:
                        //SelectByRadius(e);
                        break;

                    default:
                        break;

                }

            });

            MyMapView.GeoViewTapped += (s, e) =>
            {
                switch (currentBtnAction)
                {
                    case 1:
                        AddPoint(e.Location);
                        break;

                    case 2:
                        SkatchPolygon(e);
                        break;

                    case 3:
                        SelectByRadius(e);
                        break;

                    default:
                        break;

                }

            };


            MyMapView.MouseRightButtonUp += (s, e) =>
            {
                switch (currentBtnAction)
                {
                    case 2:
                        DrawPolygon();
                        break;
                    default:

                        break;
                }
            };



        }


        

        private List<MapPoint> drawPointActionHistory = new List<MapPoint>();
        private List<MapPoint> undoPointActionHistory = new List<MapPoint>();

        private void UndoPointAction()
        {
            if (drawPointActionNum > 0)
            {
                MapPoint loc = drawPointActionHistory.Last();
                drawPointActionHistory.Remove(loc);
                var pointsTable = pointsData.Tables.ElementAt(drawPointActionNum-- % N);
                pointsTable.DeleteFeatureAsync(pointsTable.Last());
                undoPointActionHistory.Add(loc);

            }
        }

        private async void RedoPointAction()
        {
            if (undoPointActionHistory.Count() > 0)
            {
                MapPoint location = undoPointActionHistory.Last();
                undoPointActionHistory.Remove(location);

                var pointsTable = pointsData.Tables.ElementAt(++drawPointActionNum % N);

                var pointFeature = pointsTable.CreateFeature();
                pointFeature.Geometry = location;

                pointFeature.Attributes["Text"] = $"{location.X},{location.Y}";
                try
                {
                    await pointsTable.AddFeatureAsync(pointFeature);
                    drawPointActionHistory.Add(location);
                }
                catch { }
                

            }
        }

        private int drawPointActionNum = 0;
        private async void AddPoint(MapPoint location)
        {
            var pointsTable = pointsData.Tables.ElementAt(++drawPointActionNum % N);

            var pointFeature = pointsTable.CreateFeature();
            pointFeature.Geometry = location;
            
            pointFeature.Attributes["Text"] = $"{location.X},{location.Y}";
            try
            {
                await pointsTable.AddFeatureAsync(pointFeature);
                drawPointActionHistory.Add(location);
            }
            catch { }
            undoPointActionHistory.Clear();
        }

        private List<MapPoint> undoPolygonActionHistory = new List<MapPoint>();
        private void UndoPolygonAction()
        {
            if (tempMapPoints.Count()>0)
            {
                MapPoint loc = tempMapPoints.Last();
                tempMapPoints.Remove(loc);
                undoPolygonActionHistory.Add(loc);

                tempPointGraphicsOverlay.Graphics.Remove(tempPointGraphicsOverlay.Graphics.Last());
                if (tempMapPoints.Count() == 0)
                    polygonBuilder = new PolygonBuilder(SpatialReferences.WebMercator);
                else
                    polygonBuilder = new PolygonBuilder(tempMapPoints);

                tempPolygonGraphicsOverlay.Graphics.Clear();
                var nestingGround = polygonBuilder.ToGeometry();
                var outlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Blue, 1.0);
                var fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.DiagonalCross, Color.FromArgb(255, 0, 80, 0), outlineSymbol);
                var nestingGraphic = new Graphic(nestingGround, fillSymbol);
                tempPolygonGraphicsOverlay.Graphics.Add(nestingGraphic);
            }
            
        }

        private void RedoPolygonAction()
        {
            if (undoPolygonActionHistory.Count>0)
            {
                MapPoint loc = undoPolygonActionHistory.Last();
                undoPolygonActionHistory.Remove(loc);

                polygonBuilder.AddPoint(loc);
                tempMapPoints.Add(loc);
                var tempPointMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Green, 7);
                var tempPointGraphic = new Graphic(loc, tempPointMarker);
                tempPointGraphicsOverlay.Graphics.Add(tempPointGraphic);

                tempPolygonGraphicsOverlay.Graphics.Clear();
                var nestingGround1 = polygonBuilder.ToGeometry();
                var outlineSymbol1 = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Blue, 1.0);
                var fillSymbol1 = new SimpleFillSymbol(SimpleFillSymbolStyle.DiagonalCross, Color.FromArgb(255, 0, 80, 0), outlineSymbol1);
                var nestingGraphic1 = new Graphic(nestingGround1, fillSymbol1);
                tempPolygonGraphicsOverlay.Graphics.Add(nestingGraphic1);
            }
            

        }

        private void SkatchPolygon(GeoViewInputEventArgs e)
        {
            //var identifyPolPointGraphics = await MyMapView.IdentifyGraphicsOverlayAsync(tempPointGraphicsOverlay, e.Position, 10, false);

            //if (identifyPolPointGraphics != null && identifyPolPointGraphics.Graphics.Count > 0)
            //{
            //    var _idGraphic = identifyPolPointGraphics.Graphics.FirstOrDefault();

            //    int a = tempPointGraphicsOverlay.Graphics.IndexOf(_idGraphic);
            //    tempPointGraphicsOverlay.Graphics.Remove(_idGraphic);

            //    tempMapPoints.RemoveAt(a);

            //    if (tempMapPoints.Count() == 0)
            //        polygonBuilder = new PolygonBuilder(SpatialReferences.WebMercator);
            //    else
            //        polygonBuilder = new PolygonBuilder(tempMapPoints);

            //    tempPolygonGraphicsOverlay.Graphics.Clear();
            //    var nestingGround = polygonBuilder.ToGeometry();
            //    var outlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Blue, 1.0);
            //    var fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.DiagonalCross, Color.FromArgb(255, 0, 80, 0), outlineSymbol);
            //    var nestingGraphic = new Graphic(nestingGround, fillSymbol);
            //    tempPolygonGraphicsOverlay.Graphics.Add(nestingGraphic);

            //}
            //else
            //{
                polygonBuilder.AddPoint(e.Location);
                tempMapPoints.Add(e.Location);
                var tempPointMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Green, 7);
                var tempPointGraphic = new Graphic(e.Location, tempPointMarker);
                tempPointGraphicsOverlay.Graphics.Add(tempPointGraphic);

                tempPolygonGraphicsOverlay.Graphics.Clear();
                var nestingGround1 = polygonBuilder.ToGeometry();
                var outlineSymbol1 = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Blue, 1.0);
                var fillSymbol1 = new SimpleFillSymbol(SimpleFillSymbolStyle.DiagonalCross, Color.FromArgb(255, 0, 80, 0), outlineSymbol1);
                var nestingGraphic1 = new Graphic(nestingGround1, fillSymbol1);
                tempPolygonGraphicsOverlay.Graphics.Add(nestingGraphic1);

            //}
        }

        private int drawPolygonActionNum = 0; 
        private async void DrawPolygon()
        {
            if (polygonBuilder.IsSketchValid)
            {

                var polygonsTable = polygonsData.Tables.ElementAt(++drawPolygonActionNum % N);
                var polygonFeature = polygonsTable.CreateFeature();
                polygonFeature.Geometry = polygonBuilder.ToGeometry();
                polygonFeature.Attributes["Text"] = $"{polygonBuilder.ToGeometry().ToJson()}";
                try
                {
                    await polygonsTable.AddFeatureAsync(polygonFeature);
                }
                catch { }
            }
            tempMapPoints = new List<MapPoint>();
            undoPolygonActionHistory.Clear();
            polygonBuilder = new PolygonBuilder(SpatialReferences.WebMercator);
            tempPointGraphicsOverlay.Graphics.Clear();
            tempPolygonGraphicsOverlay.Graphics.Clear();
        }

        private async void SelectByRadius(Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Build a buffer (polygon) around a click point
            var buffer = GeometryEngine.Buffer(e.Location, 5000000);


            // Use the buffer to define the geometry for a query
            var query = new QueryParameters();
            query.Geometry = buffer;
            query.SpatialRelationship = SpatialRelationship.Intersects;

            string str = "";
            foreach (var table in polygonsData.Tables)
            {
                await table.FeatureLayer.SelectFeaturesAsync(query, Esri.ArcGISRuntime.Mapping.SelectionMode.New);
                var selectedFeatures = await table.FeatureLayer.GetSelectedFeaturesAsync();

                foreach (var f in selectedFeatures)
                {
                    //await polygonsData.Tables.First().DeleteFeatureAsync(f);
                    str = str + f.Geometry.ToJson() + '\n';

                }

            }
            InfoText = str;
        }



    }



}