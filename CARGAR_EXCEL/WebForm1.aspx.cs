using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.IO;
using System.Data;
using System.Data.SqlClient;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Globalization;

namespace CARGAR_EXCEL
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Globalization.CultureInfo.CurrentCulture = new CultureInfo("es-PE");
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string ruta_carpeta = HttpContext.Current.Server.MapPath("~/Temporal");

            if (!Directory.Exists(ruta_carpeta))
            {
                Directory.CreateDirectory(ruta_carpeta);
            }

            //GUARDAMOS EL ARCHIVO EN LOCAL
            var ruta_guardado = Path.Combine(ruta_carpeta, FileUpload1.FileName);
            FileUpload1.SaveAs(ruta_guardado);


            IWorkbook MiExcel = null;
            FileStream fs = new FileStream(ruta_guardado, FileMode.Open, FileAccess.Read);

            if (Path.GetExtension(ruta_guardado) == ".xlsx")
                MiExcel = new XSSFWorkbook(fs);
            else
                MiExcel = new HSSFWorkbook(fs);


            ISheet hoja = MiExcel.GetSheetAt(0);

            DataTable table = new DataTable();
            table.Columns.Add("data1", typeof(string));
            table.Columns.Add("data2", typeof(string));
            table.Columns.Add("data3", typeof(string));
            table.Columns.Add("data4", typeof(string));

            if (hoja != null) {

                int cantidadfilas = hoja.LastRowNum;

                for (int i = 1; i <= cantidadfilas; i++) {
                    IRow fila = hoja.GetRow(i);


                    if(fila != null)
                        table.Rows.Add(
                            fila.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK) != null ? fila.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK).NumericCellValue.ToString() : "",
                            fila.GetCell(1, MissingCellPolicy.RETURN_NULL_AND_BLANK) != null ? fila.GetCell(1, MissingCellPolicy.RETURN_NULL_AND_BLANK).ToString() : "",
                            fila.GetCell(2, MissingCellPolicy.RETURN_NULL_AND_BLANK) != null  ? fila.GetCell(2, MissingCellPolicy.RETURN_NULL_AND_BLANK).DateCellValue.ToString("dd/MM/yyyy",new CultureInfo("es-ES")) : "",
                             fila.GetCell(3, MissingCellPolicy.RETURN_NULL_AND_BLANK) != null ? fila.GetCell(3, MissingCellPolicy.RETURN_NULL_AND_BLANK).NumericCellValue.ToString() : ""
                            );
                }
            }

            GridView1.DataSource = table;
            GridView1.DataBind();
          
            int resultado = cargarEnSQL(table);

            if (resultado == 1) {
                GridView1.DataSource = table;
                GridView1.DataBind();
            }

        }




        public int cargarEnSQL(DataTable tabla)
        {
            int resultado = 0;
            try
            {
                //NOS CONECTAMOS CON LA BASE DE DATOS
                using (SqlConnection cn = new SqlConnection("Data Source=.;Initial Catalog=Pruebas ;Integrated Security=True"))
                {
                    SqlCommand cmd = new SqlCommand("usp_cargarInformacion", cn);
                    cmd.Parameters.Add("EstructuraCarga", SqlDbType.Structured).Value = tabla;
                    cmd.Parameters.Add("Resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cn.Open();
                    cmd.ExecuteNonQuery();
                    resultado = Convert.ToInt32(cmd.Parameters["Resultado"].Value);
                }
            }
            catch (Exception ex)
            {

                string mensaje = ex.Message.ToString();
                resultado = 0;
            }

            return resultado;
        }

      
    }

    public class informacion {
        public string data1 { get; set; }
        public string data2 { get; set; }
        public string data3 { get; set; }
        public string data4 { get; set; }
    }
}