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
using System.Data.SqlClient;

namespace DSA_lims
{
    public partial class FormShowResultMap : Form
    {
        List<int> mSampleList = null;
        private GMapControl gmap = new GMapControl();
        private GMapOverlay overlay = new GMapOverlay();

        public FormShowResultMap(List<int> sampleList)
        {
            InitializeComponent();

            lblInfo.Text = "";
            mSampleList = sampleList;
            panelGMap.Controls.Add(gmap);
            gmap.Dock = DockStyle.Fill;
        }        

        private void FormShowResultMap_Load(object sender, EventArgs e)
        {
            cboxProviders.Items.Add(ArcGIS_World_Topo_MapProvider.Instance);
            cboxProviders.Items.Add(OpenStreetMapProvider.Instance);
            cboxProviders.Items.Add(BingMapProvider.Instance);
            cboxProviders.Items.Add(BingSatelliteMapProvider.Instance);
            cboxProviders.Items.Add(BingHybridMapProvider.Instance);
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

            gmap.Position = new PointLatLng(62.7949348788701, 10.72265625);
            gmap.Zoom = 5;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            PopulateMarkers();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void FormShowResultMap_FormClosing(object sender, FormClosingEventArgs e)
        {
            gmap.Manager.CancelTileCaching();
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

        private void PopulateMarkers()
        {
            RemoveAllMarkers();
            int nSamplesShown = 0;

            using (SqlConnection conn = DB.OpenConnection())
            {
                string query = @"
select s.latitude, s.longitude, st.name as 'sample_type_name'
from sample s
    inner join sample_type st on st.id = s.sample_type_id
where number = @id
";
                SqlCommand cmd = new SqlCommand(query, conn);

                foreach (int id in mSampleList)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if(reader.HasRows)
                        {
                            reader.Read();

                            if (DB.IsValidField(reader["latitude"]) && DB.IsValidField(reader["longitude"]))
                            {
                                double lat = reader.GetDouble("latitude");
                                double lon = reader.GetDouble("longitude");
                                string stype = reader.GetString("sample_type_name");
                                GMapMarker marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                                    new PointLatLng(lat, lon),
                                    GMap.NET.WindowsForms.Markers.GMarkerGoogleType.yellow_pushpin);

                                marker.ToolTipText = "Sample: " + id + Environment.NewLine + 
                                    "Sample type: " + stype + Environment.NewLine +
                                    "Latitude: " + lat + Environment.NewLine + 
                                    "Longitude: " + lon;
                                overlay.Markers.Add(marker);
                                nSamplesShown++;
                            }
                        }
                    }
                }                                
            }

            lblInfo.Text = mSampleList.Count + " samples, showing " + nSamplesShown + " with coordinates";
            gmap.Refresh();
        }
    }
}
