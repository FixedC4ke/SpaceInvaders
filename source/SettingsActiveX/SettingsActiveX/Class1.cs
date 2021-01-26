using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace SettingsActiveX
{
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("D421E083-2C36-4BB3-9DA7-8814AE0C9B9D")]
    public interface _InterfaceSettings
    {
        Dictionary<string, string> getSettings(int numLevel);
    }

    [ClassInterface(ClassInterfaceType.None)]
    [Guid("70010775-46B2-449C-BF06-BAC0E7613F87")]
    public class pdf_Reader : _InterfaceSettings
    {
        public Dictionary<string, string> getSettings(int numLevel)
        {
            Dictionary<string, string> Settings = new Dictionary<string, string>();

            Excel.Application ex = new Microsoft.Office.Interop.Excel.Application();
            Excel.Application ObjWorkExcel = new Excel.Application(); // Объявляем приложение
            Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(Environment.CurrentDirectory+"\\Settings.xlsx", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing); // Открываем файл

            // ex.Visible = true; // Отобразить Excel
            // ex.DisplayAlerts = false; // Отключить отображение окон с сообщениями

            int countLevels = ObjWorkBook.Sheets.Count; // Количество уровней 

            // Получаем настройки с листов документа (счет начинается с 1)

            Excel.Worksheet ObjWorkSheet = (Excel.Worksheet)ObjWorkBook.Sheets[numLevel]; // Переходим к листу
            var lastCell = ObjWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell); // Последняя ячейка

            for (int j = 0; j < (int)lastCell.Row; j++) // По всем строкам
            {
                string nameSetting = ObjWorkSheet.Cells[j + 1, 1].Text.ToString(); // Название параметра
                string valueSetting = ObjWorkSheet.Cells[j + 1, 2].Text.ToString(); // Значение параметра 

                Settings.Add(nameSetting, valueSetting);
            }



            ObjWorkExcel.Quit();
            GC.Collect();
            // System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ex);
            return Settings;
        }
    }
}
