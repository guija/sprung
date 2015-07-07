using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sprung
{
    class SprungLayout
    {
        private DataGridView matchingBox;
        private DataTable layout;

        public SprungLayout(DataGridView matchingBox)
        {
            this.matchingBox = matchingBox;
            this.layout = new DataTable();
        }

        public void addImageColumn(string name, int width)
        {
            var imageCol = new DataGridViewImageColumn();
            imageCol.DataPropertyName = name;
            this.layout.Columns.Add(name, typeof(Image));
            this.matchingBox.Columns.Add(imageCol);
            this.matchingBox.Columns[matchingBox.Columns.Count - 1].Width = width;
        }

        public void addTextColumn(string name, int width)
        {
            var textCol = new DataGridViewTextBoxColumn();
            textCol.DataPropertyName = name;
            this.layout.Columns.Add(name);
            this.matchingBox.Columns.Add(textCol);
            this.matchingBox.Columns[matchingBox.Columns.Count - 1].Width = width;
        }

        public void setNotSortable(bool notSortable)
        {
            if (!notSortable)
            {
                for (int i = 0; i < this.matchingBox.Columns.Count; i++)
                {
                    this.matchingBox.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }
        }

        public void addProcesses(List<Window> windows)
        {
            foreach (Window window in windows)
            {
                if (!(window.getProcessName() == "") && !(window.getProcessTitle() == ""))
                {
                    this.layout.Rows.Add(null, window.getProcessTitle());
                }
            }
        }

        public void setSelectionMode(System.Windows.Forms.DataGridViewSelectionMode dataGridViewSelectionMode)
        {
            this.matchingBox.SelectionMode = dataGridViewSelectionMode;
        }

        public void setMultiSelect(bool multiSelect)
        {
            this.matchingBox.MultiSelect = multiSelect;
        }

        public void setReadOnly(bool readOnly)
        {
            this.matchingBox.ReadOnly = readOnly;
        }

        public void setScrolls(ScrollBars scrollBars)
        {
            this.matchingBox.ScrollBars = scrollBars;
        }

        public void setResizeable(bool resizeable)
        {
            this.matchingBox.AllowUserToResizeColumns = resizeable;
            this.matchingBox.AllowUserToResizeRows = resizeable;
        }

        public DataTable getTable()
        {
            return this.layout;
        }
    }
}
