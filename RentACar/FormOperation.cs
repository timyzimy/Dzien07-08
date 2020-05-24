using MySql.Data.MySqlClient;
using RentACar.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RentACar
{
    public partial class FormOperation : Form
    {
        public int CarId { get; set; } = 0;
        public String RegPlate { get; set; } = "";
        public bool OperBack { get; set; } = false; // jesli true, to zwrot 

        private int lastRecordId = -1;

        public FormOperation()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormOperation_Load(object sender, EventArgs e)
        {
            if (OperBack)
            {
                this.Text = "Zwrot pojazdu" + this.RegPlate;

                try
                {
                    String sql = "SELECT id, description FROM operations where car_id=" + CarId + " ORDER BY id DESC LIMIT 0,1 ";
                    MySqlDataAdapter adapter = new MySqlDataAdapter();
                    adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        lastRecordId = Convert.ToInt32(dt.Rows[0]["id"]);
                        tbDescr.Text = dt.Rows[0]["description"].ToString();
                    }
                } catch (Exception exc)
                {
                    DialogHelper.Error(exc.Message);
                }


            } else
            {
                this.Text = "Wydanie pojazdu " + this.RegPlate;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            MySqlTransaction tr = null;
            try
            {
                String sql = "";
                int avail = 0;
                if (!OperBack)
                {
                    //zakladamy nowy rekord
                    sql = @"
                    INSERT INTO operations
                     (car_id, ts_out, mileage_out, description)
                        VALUES
                     (@car_id, @ts, @mileage, @descr)
                ";
                }
                else
                {
                    //aktualizujemy ostatni rekord operacji
                    sql = @" UPDATE operations SET
                             ts_in=@ts, mileage_in=@mileage, description=@descr
                                WHERE id=@id
                        ";
                    avail = 1;
                }

                tr = GlobalData.connection.BeginTransaction();

                MySqlCommand cmd = new MySqlCommand(sql, GlobalData.connection);
                cmd.Transaction = tr;
                cmd.Parameters.Add("@car_id", MySqlDbType.Int32).Value = CarId;
                cmd.Parameters.Add("@ts", MySqlDbType.DateTime).Value = dtDate.Value;
                cmd.Parameters.Add("@mileage", MySqlDbType.Int32).Value = numMileage.Value;
                cmd.Parameters.Add("@descr", MySqlDbType.Text).Value = tbDescr.Text;
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = lastRecordId;
                cmd.ExecuteNonQuery();

                // update kolumny avail w tablicy 'cars'
                sql = "UPDATE cars SET avail=@avail WHERE id=@id";
                cmd = new MySqlCommand(sql, GlobalData.connection);
                cmd.Transaction = tr;
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = CarId;
                cmd.Parameters.Add("@avail", MySqlDbType.Int32).Value = avail;
                cmd.ExecuteNonQuery();

                tr.Commit();

                DialogResult = DialogResult.OK;
                Close();
            
            } catch (Exception exc)
            {
                if (tr != null)
                {
                    tr.Rollback();
                }

                DialogHelper.Error(exc.Message);
            }

        }
    }
}
