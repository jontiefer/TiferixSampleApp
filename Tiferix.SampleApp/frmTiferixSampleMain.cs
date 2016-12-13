/* Copyright © 2016 Jonathan Tiefer - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the GNU Lesser General Public License (LGPL)
 *
 * You should have received a copy of the LGPL license with
 * this file.
 *
 * /

/*  This file is part of Tiferix Sample Application.
*
*   The Tiferix Sample Application is free software: you can redistribute it and/or modify
*   it under the terms of the GNU Lesser General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*    (at your option) any later version.
*
*   Tiferix Sample Application is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU Lesser General Public License for more details.
*
*  You should have received a copy of the GNU Lesser General Public License
*   along with the Tiferix Sample Application.  If not, see <http://www.gnu.org/licenses/>.
*/

/* Additional Credits:
 * TiferixSoftwareSample logos created by FlameText logo design.  The FlameText logo website address is http://www4.flamingtext.com.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using Tiferix.Global;
using DotCoolControls.Tools;
using DotCoolControls.WinForms;
using Tiferix.DotCoolGrid;
using Tiferix.Json.Serialization;
using Tiferix.Json.Data;
using Tiferix.Json.Tools;


namespace Tiferix.SampleApp
{
    /// <summary>
    /// TiferixSampleMain form is the main form of the Tiferix Software Sample application which utilizes both the DotCoolControls, DotCoolGridView 
    /// and the Tiferix.Json library.
    /// </summary>
    public partial class frmTiferixSampleMain : Form
    {
        #region Member Variables
        #endregion

        #region Member Object Variables
        #endregion

        #region Data Object Variables

        private DataSet m_dsJsonData = null;

        private List<DotCoolRadioButton> m_lstRBTableCtls = new List<DotCoolRadioButton>();

        #endregion

        #region Construction/Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public frmTiferixSampleMain()
        {
            try
            {                       
                InitializeComponent();

                gridDotCool.Columns.Remove("TemplateCol");
                
                string strDefSchemaFile = ApplicationPath + @"\Schema\SampleDataSchema.json";
                string strDefDataFile = ApplicationPath + @"\Data\SampleData.json";

                if (File.Exists(strDefSchemaFile))
                    txtJsonSchemaFileName.Text = strDefSchemaFile;

                if (File.Exists(strDefDataFile))
                    txtJsonDataFileName.Text = strDefDataFile;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor function of frmTiferixSampleMain form.");
            }
        }

        #endregion

        #region General Properties, Functions
        
        /// <summary>
        /// Path to the executable of the application, excluding the name of the executable.  This version of ApplicationPath will return the application
        /// path, regardless if it is Debug or Release mode, assuming that the debug executable is stored in the path of bin\Debug.
        /// </summary>
        public static string ApplicationPath
        {
            get
            {
                string strAppPath = General.ApplicationPath;

#if DEBUG
                if (strAppPath.Contains("\\bin\\Debug"))
                    strAppPath = strAppPath.Replace("\\bin\\Debug", "");
#endif

                return strAppPath;
            }
        }

        #endregion

        #region Json DataSet Schema Loading and Data Schema-Related UI Properties, Functions, Event Handlers

        /// <summary>
        /// Uses a Tiferix.JsonDataSetSchemaSerializer object to load and deserialize a Tiferix.JsonDataSetSchema file from disk into a memory 
        /// resident ADO.Net DataSet object.  The Tiferix.JsonDataSetSchema file contains all schema information required to intialize the DataSet.  
        /// Before a Tiferix.JsonDataSet file can be loaded into a DataSet, it will be required that the DataSet's schema is first loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadJsonSchema_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(txtJsonSchemaFileName.Text))
                {
                    MessageBox.Show("Json Schema file is either invalid or the file is not found.", "Invalid Json Schema File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    txtJsonSchemaFileName.Focus();
                    return;
                }//end if

                JsonDataSetSchemaSerializer serSchema = new JsonDataSetSchemaSerializer();
                
                if (!serSchema.DeserializeJsonDataSetSchema(txtJsonSchemaFileName.Text, ref m_dsJsonData))
                {
                    MessageBox.Show("Json DataSet Schema file specified is invalid.", "Invalid Schema File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }//end if

                InitializeDataTableUI();
                
                gridDotCool.DataSource = null;
                gridDotCool.DataMember = "";
                
                gridDotCool.DataSource = m_dsJsonData;
                m_lstRBTableCtls[0].Checked = true;
                
                MessageBox.Show("Successfully loaded Json DataSet Schema file.", "Json Schema Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in btnLoadJsonSchema_Click function of frmTiferixSampleMain form.");
            }
        }

        /// <summary>
        /// Displays an open file picker dialog that allows the user to select the Tiferix.JsonDataSetSchema file to use to initialize the DataSet used 
        /// in the TiferixSampleMain form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchJsonSchema_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlgOpen = new OpenFileDialog();
                dlgOpen.Filter = "Json File (.json)|*.json|All Files (*.*)|*.*";                
                dlgOpen.FilterIndex = 0;                

                if (dlgOpen.ShowDialog() == DialogResult.OK)
                {
                    txtJsonSchemaFileName.Text = dlgOpen.FileName;
                }//end if
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in btnSearchJsonSchema_Click function of frmTiferixSampleMain form.");
            }
        }

        /// <summary>
        /// When the DataSet used in the TiferixSampleMain is loaded with the appropriate schema information from the Tiferix.JsonDataSetSchema file, 
        /// then all tables contained in the DataSet will be loaded as DotCoolRadioButtons in the Tables panel of the form.  All previously loaded 
        /// tables DotCoolRadioButtons will be cleared before loading the new set of table DotCoolRadioButton controls.
        /// </summary>
        private void InitializeDataTableUI()
        {
            try
            {
                //Clears all previous DataTable DotCoolRadioButtons that were loaded from a previous dataset from the form.
                foreach (DotCoolRadioButton rbTable in m_lstRBTableCtls)
                {
                    rbTable.Checked = false;
                    rbTable.Visible = false;
                    rbTable.CheckChanged -= new EventHandler(rbTable_CheckedChanged);
                    pnlTables.Controls.Remove(rbTable);
                    rbTable.Dispose();                    
                }//next rbTable

                m_lstRBTableCtls.Clear();

                Point ptTableRBLocation = rbTableTemplate.Location;

                foreach (DataTable dtTable in m_dsJsonData.Tables)
                {
                    DotCoolRadioButton rbTable = rbTableTemplate.Clone();                    
                    rbTable.Name = "rbTable_" + dtTable.TableName;
                    rbTable.Text = dtTable.TableName;                                       
                    rbTable.CheckChanged += new EventHandler(rbTable_CheckedChanged);

                    pnlTables.Controls.Add(rbTable);
                    rbTable.Location = ptTableRBLocation;
                    rbTable.Visible = true;

                    m_lstRBTableCtls.Add(rbTable);
                    ptTableRBLocation.Y += rbTableTemplate.Height + 1;
                }//next dtTable
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in InitializeDataTableUI function of frmTiferixSampleMain form.");
            }
        }

        #endregion

        #region Json DataSet Data Loading and Data-Related UI Loading Properties, Functions, Event Handlers

        /// <summary>
        /// Uses a Tiferix.JsonDataSetSerializer object to laod and deserialize a Tiferix.JsonDataSet data file from disk into a memory resident ADO.Net
        /// DataSet object.  Before the DataSet can be loaded with data from the Tiferix.JsonDataSet file, it will be required that the DataSet is first 
        /// initialized with the appropriate schema that matches the data in the Tiferix.JsonDataSet data file.  After the schema is loaded, then the 
        /// JsonDataSetSerializer class will load and deserialize all the data stored in the Tiferix.JsonDataSet file into their associated tables of the
        /// form's DataSet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadJsonData_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(txtJsonDataFileName.Text))
                {
                    MessageBox.Show("Json DataSet Data file is either invalid or the file is not found.", "Invalid Json DataSet Data File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    txtJsonDataFileName.Focus();
                    return;
                }//end if

                gridDotCool.SuspendLayout();

                JsonDataSetSerializer serData = new JsonDataSetSerializer();
                serData.DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ssZ");

                if (!serData.DeserializeJsonDataSet(txtJsonDataFileName.Text, ref m_dsJsonData))
                {
                    MessageBox.Show("Json DataSet Data file specified is invalid.", "Invalid Data File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }//end if

                gridDotCool.ResumeLayout();
                gridDotCool.Refresh();

                MessageBox.Show("Successfully loaded Json DataSet Data file.", "Json Data Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in btnLoadJsonData_Click function of frmTiferixSampleMain form.");
                gridDotCool.ResumeLayout();
            }
        }

        /// <summary>
        /// Displays an open file picker dialog that allows the user to select the Tiferix.JsonDataSet file containing all the data that will be loaded in the 
        /// DataSet used in the TiferixSampleMain form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchJsonData_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlgOpen = new OpenFileDialog();
                dlgOpen.Filter = "Json File (.json)|*.json|All Files (*.*)|*.*";
                dlgOpen.FilterIndex = 0;

                if (dlgOpen.ShowDialog() == DialogResult.OK)
                {
                    txtJsonDataFileName.Text = dlgOpen.FileName;
                }//end if
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in btnSearchJsonData_Click function of frmTiferixSampleMain form.");
            }
        }

        #endregion

        #region DotCoolGridView Table/Record Selection and Display Properties, Functions, Event Handlers

        /// <summary>
        /// Handles the event when one of the Table DotCoolRadioButtons in the Tables panel of the form is checked or unchecked.  The Table
        /// DotCoolRadioButton that is selected in the form will be the table in the DataSet that will be displayed in the DotCoolGrid of the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbTable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                DotCoolRadioButton rbTable = (DotCoolRadioButton)sender;

                if (rbTable.Checked)
                {
                    gridDotCool.DataMember = rbTable.Text;
                    gridDotCool.Refresh();

                    UpdateDotCoolGridLayout();
                }//end if
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error inrbTable_CheckedChanged function of frmTiferixSampleMain form.");
            }
        }

        /// <summary>
        /// This function will update the layout and formatting of certain types of columns in the DotCoolGrid of the form when a new data table is 
        /// bound to the form's DotCoolGrid.
        /// </summary>
        private void UpdateDotCoolGridLayout()
        {
            try
            {
                foreach (DataGridViewColumn column in gridDotCool.Columns)
                {
                    if (column.Name.ToUpper().Contains("NAME"))
                    {
                        column.Width = 175;
                    }
                    else if (column.Name.ToUpper().Contains("PHONE"))
                    {                        
                        column.Width = 100;                                                
                    }//end if

                    if (column.ValueType == typeof(DateTime))
                    {
                        column.Width = 100;
                        column.DefaultCellStyle.Format = "MM/dd/yyyy";                        
                    }//end if
                }//next column
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in UpdateDotCoolGridLayout function of frmTiferixSampleMain form.");
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event for the various DotCoolCheckBox controls in the form that serve as filters for the DotCoolGrid of the form. 
        /// Any time a filter checkbox is checked or unchecked, the appropriate settings will be updated in the form's DotCoolGrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbGridFilter_CheckChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender == chbDispBackImage)
                {
                    gridDotCool.DrawBackgroundImage = chbDispBackImage.Checked;

                    if (chbDispBackImage.Checked)
                        chbDispBackGradient.Checked = false;
                }
                else if (sender == chbDispBackGradient)
                {
                    gridDotCool.DrawBackgroundGradient = chbDispBackGradient.Checked;

                    if (chbDispBackGradient.Checked)
                        chbDispBackImage.Checked = false;
                }
                else if (sender == chbEnableColHdrStyles)
                {
                    gridDotCool.DrawColHeaderGradient = chbEnableColHdrStyles.Checked;
                }
                else if (sender == chbEnableTrans)
                {
                    gridDotCool.DrawCellTransColor = chbEnableTrans.Checked;
                    gridDotCool.DrawSelectedCellTransColor = chbEnableTrans.Checked;
                }//end if
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in chbGridFilter_CheckChanged function of frmTiferixSampleMain form.");
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of the Translucency Alpha Blending trackbar in the form.  The trackbar will modify the translucency alpha 
        /// blending settings of the DotCoolGrid in the form when translucency is used in the grid.  The valid range of values will be from 0 (Fully Transparent) 
        /// to 255 (Fully Opaque).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTransAlpha_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                gridDotCool.CellTransAlpha = tbTransAlpha.Value;

                int iSelCellTransAlpha = tbTransAlpha.Value + 10;

                if (iSelCellTransAlpha > 255)
                    iSelCellTransAlpha = 255;

                gridDotCool.SelectedCellTransAlpha = iSelCellTransAlpha;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in tbTransAlpha_ValueChanged function of frmTiferixSampleMain form.");
            }
        }

        /// <summary>
        /// Handles the click event of the form's navigation DotCoolButton controls that will be used to navigate through the records loaded in the
        /// form's DotCoolGrid control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveRec_Click(object sender, EventArgs e)
        {
            try
            {
                int iSelRowIndex = 0;

                if (gridDotCool.Rows.Count == 0)
                    return;

                if (gridDotCool.SelectedRows.Count > 0)
                    iSelRowIndex = gridDotCool.SelectedRows[0].Index;

                gridDotCool.ClearSelection();

                if (sender == btnMoveFirst)
                {                    
                    gridDotCool.Rows[0].Selected = true;
                }
                else if (sender == btnMovePrev)
                {
                    if (iSelRowIndex > 0)
                        iSelRowIndex--;

                    gridDotCool.Rows[iSelRowIndex].Selected = true;
                }
                else if (sender == btnMoveLast)
                {
                    gridDotCool.Rows[gridDotCool.Rows.Count - 1].Selected = true;
                }
                else if (sender == btnMoveNext)
                {
                    if (iSelRowIndex < gridDotCool.Rows.Count - 1)
                        iSelRowIndex++;

                    gridDotCool.Rows[iSelRowIndex].Selected = true;
                }//end if

                gridDotCool.FirstDisplayedScrollingRowIndex = gridDotCool.SelectedRows[0].Index;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in btnMoveRec_Click function of frmTiferixSampleMain form.");
            }
        }

        #endregion

        #region DotCoolGridView Data Editing and General UI Specific Functions, Event Handlers

        /// <summary>
        /// Handles the CellValidating event of the DotCoolGrid control.  Certain cells, like date cells will be validated to make sure the proper date 
        /// format is entered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridDotCool_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            try
            {
                if (e.FormattedValue == null)
                    return;

                if (gridDotCool.Columns[e.ColumnIndex].ValueType == typeof(DateTime) && e.FormattedValue.ToString() != "")
                {
                    DateTimeFormat format = new DateTimeFormat("MM/dd/yyyy");
                    DateTime datVal;
                    if (!DateTime.TryParse(e.FormattedValue.ToString(), out datVal))
                    {
                        MessageBox.Show("Invalid Date Format!", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        e.Cancel = true;
                    }//end if                    
                }//end if                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in gridDotCool_CellValidating function of frmTiferixSampleMain form.");
            }
        }

        #endregion

        #region DataSet Export Properties, Functions, Event Handlers

        /// <summary>
        /// Uses the Tiferix.JsonDataSetSerializer class to export the current data stored in the form's DataSet to a Tiferix.JsonDataSet data file.  A 
        /// file save dialog will be displayed to allow the user to select the path and name of the exported Tiferix.JsonDataSet data file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportJsonData_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlgSave = new SaveFileDialog();
                dlgSave.Filter = "Json File (.json)|*.json|All Files (*.*)|*.*";
                dlgSave.FilterIndex = 0;

                if (dlgSave.ShowDialog() == DialogResult.OK)
                {
                    JsonDataSetSerializer serData = new JsonDataSetSerializer();
                    serData.DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ssZ");

                    serData.SerializeDataSet(dlgSave.FileName, m_dsJsonData);
                    serData.Dispose();

                    if (File.Exists(dlgSave.FileName))
                    {
                        MessageBox.Show("Json DataSet Data file successfully exported!", "Json Data Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }//end if
                }//end if
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in btnExportJsonData_Click function of frmTiferixSampleMain form.");
            }
        }

        /// <summary>
        /// Exports the current data stored in the form's DataSet using the DataSet's WriteXML function to an XML data file.  A file save dialog will be
        /// displayed to allow the user to select the path and name of the exported XML file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportXMLData_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlgSave = new SaveFileDialog();
                dlgSave.Filter = "XML File (.xml)|*.xml|All Files (*.*)|*.*";
                dlgSave.FilterIndex = 0;

                if (dlgSave.ShowDialog() == DialogResult.OK)
                {
                    m_dsJsonData.WriteXml(dlgSave.FileName);

                    if (File.Exists(dlgSave.FileName))
                    {
                        MessageBox.Show("XML DataSet Data file successfully exported!", "XML Data Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }//end if
                }//end if
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in btnExportXMLData_Click function of frmTiferixSampleMain form.");
            }
        }

        #endregion

        #region Form General Functions, Event Handlers

        /// <summary>
        /// Handles the Resize event of the DotCoolSample form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmTiferixSampleMain_Resize(object sender, EventArgs e)
        {
            try
            {
                pnlJsonSchema.Width = Convert.ToInt32(this.Width * 0.48);
                pnlJsonData.Width = Convert.ToInt32(this.Width * 0.48);
                pnlJsonData.Left = this.Width - pnlJsonData.Width - 22;
            }
            catch (Exception)
            {                
            }
        }

        #endregion

        #region Form Command Functions, Event Handlers

        /// <summary>
        /// When the Exit button is clicked, the form will be closed and application terminated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in btnExit_Click function of frmTiferixSampleMain form.");
            }
        }

        /// <summary>
        /// Handles the FormClosing event of the DotCoolSample form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmTiferixSampleMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in frmTiferixSampleMain_FormClosing function of frmTiferixSampleMain form.");
            }
        }

        #endregion        
    }
}
