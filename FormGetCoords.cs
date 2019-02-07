/*	
	DSA Lims - Laboratory Information Management System
    Copyright (C) 2018  Norwegian Radiation Protection Authority

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
// Authors: Dag Robole,

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.MapProviders;

namespace DSA_lims
{
    public partial class FormGetCoords : Form
    {
        private GMapControl gmap = new GMapControl();
        private GMapOverlay overlay = new GMapOverlay();
        private GMapMarker marker = null;

        private double? CurrentLatitude;
        private double? CurrentLongitude;

        public double? SelectedLatitude { get; set; }
        public double? SelectedLongitude { get; set; }

        public FormGetCoords(double? currentLatitude = null, double? currentLongitude = null)
        {
            InitializeComponent();

            panelGMap.Controls.Add(gmap);
            gmap.Dock = DockStyle.Fill;

            CurrentLatitude = currentLatitude;
            CurrentLongitude = currentLongitude;

            SelectedLatitude = null;
            SelectedLongitude = null;
        }

        private void FormGetCoords_Load(object sender, EventArgs e)
        {
            cboxProviders.Items.Add(ArcGIS_World_Topo_MapProvider.Instance);
            cboxProviders.Items.Add(OpenStreetMapProvider.Instance);            
            cboxProviders.Items.Add(BingMapProvider.Instance);
            cboxProviders.Items.Add(BingSatelliteMapProvider.Instance);
            cboxProviders.Items.Add(BingOSMapProvider.Instance);
            cboxProviders.Items.Add(GoogleMapProvider.Instance);
            cboxProviders.Items.Add(GoogleSatelliteMapProvider.Instance);
            cboxProviders.Items.Add(GoogleTerrainMapProvider.Instance);
            cboxProviders.Items.Add(YandexMapProvider.Instance);

            if (!String.IsNullOrEmpty(Common.Settings.MapProviderName))
            {
                int idx = cboxProviders.FindStringExact(Common.Settings.MapProviderName);
                if (idx != -1)
                {
                    cboxProviders.SelectedIndex = idx;
                    gmap.MapProvider = (GMapProvider)cboxProviders.SelectedItem;
                }
                else
                {
                    cboxProviders.SelectedItem = GoogleTerrainMapProvider.Instance;
                    gmap.MapProvider = (GMapProvider)cboxProviders.SelectedItem;
                }
            }
            else
            {
                cboxProviders.SelectedItem = GoogleTerrainMapProvider.Instance;
                gmap.MapProvider = (GMapProvider)cboxProviders.SelectedItem;
            }

            gmap.RoutesEnabled = false;
            gmap.CanDragMap = true;
            gmap.ScaleMode = ScaleModes.Integer;
            gmap.MouseWheelZoomEnabled = true;
            gmap.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            gmap.MinZoom = 2;
            gmap.MaxZoom = 18;            
            gmap.Overlays.Add(overlay);

            if (CurrentLatitude != null && CurrentLongitude != null)
            {
                gmap.Position = new PointLatLng(CurrentLatitude.Value, CurrentLongitude.Value);
                gmap.Zoom = 10;
                SetMarker(CurrentLatitude.Value, CurrentLongitude.Value);
            }
            else
            {
                gmap.Position = new PointLatLng(62.7949348788701, 10.72265625);
                gmap.Zoom = 5;
            }

            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            gmap.MouseClick += Gmap_MouseClick;
        }

        private void Gmap_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                double lat = gmap.FromLocalToLatLng(e.X, e.Y).Lat;
                double lon = gmap.FromLocalToLatLng(e.X, e.Y).Lng;

                SetMarker(lat, lon);
            }
        }

        private void FormGetCoords_FormClosing(object sender, FormClosingEventArgs e)
        {
            gmap.Manager.CancelTileCaching();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (marker == null)
            {
                MessageBox.Show("You must select a position first");
                return;
            }
                
            SelectedLatitude = marker.Position.Lat;
            SelectedLongitude = marker.Position.Lng;

            if(cboxProviders.SelectedItem != null)
                Common.Settings.MapProviderName = cboxProviders.SelectedItem.ToString();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cboxProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboxProviders.SelectedItem == null)
                return;

            gmap.MapProvider = (GMapProvider)cboxProviders.SelectedItem;
        }

        private void RemoveAllMarkers()
        {
            for (int i = 0; i < overlay.Markers.Count; i++)
                overlay.Markers.RemoveAt(i);
            overlay.Markers.Clear();
            overlay.Clear();
            gmap.Overlays.Remove(overlay);

            overlay = new GMapOverlay();
            gmap.Overlays.Add(overlay);        
        }        

        private void SetMarker(double lat, double lon)
        {
            RemoveAllMarkers();

            marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                new PointLatLng(lat, lon),
                GMap.NET.WindowsForms.Markers.GMarkerGoogleType.red_pushpin);
            
            overlay.Markers.Add(marker);

            gmap.Refresh();
        }
    }
}
