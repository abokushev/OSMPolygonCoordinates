using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OSMPolygonCoordinates
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Ввод данных пользователем
            Console.WriteLine("Адрес для поиска полигона: ");
            string address = Console.ReadLine();
            bool isInt32 = false;
            int frequency = 1;
            while (!isInt32)
            {
                Console.WriteLine("Частота точек: ");
                string frequencyInput = Console.ReadLine();                
                isInt32 = Int32.TryParse(frequencyInput, out frequency) && frequency > 0;
                if(!isInt32)
                {
                    Console.WriteLine("Введите корректную частоту");
                }
            }
            Console.WriteLine("Имя файла для сохранения результата: ");
            string fileName = Console.ReadLine();
            #endregion
            #region Получение и сохранение данных
            WebClient _webClient = new WebClient { Encoding = Encoding.UTF8 };

            //Для прохождения фильтрации клиентов веб-сервером.
            _webClient.Headers["User-Agent"] = "Mozilla/5.0";

            var dataFromRequest = _webClient.DownloadString("https://nominatim.openstreetmap.org/search.php?q=" + Uri.EscapeDataString(address) + "&polygon_geojson=1&format=jsonv2");

            //Проверка на частоту точек. Если пользователь не хочет уменьшать количество точек - то выполнение кода в условии не нужно.
            if (frequency != 1)
            {
                JArray json = JArray.Parse(dataFromRequest);

                foreach (var child in json)
                {
                    JArray coordinates = (JArray)child["geojson"]["coordinates"];

                    //Проверка на тип полигона. MultiPolygon является коллекцией Polygon. Если придёт тип Point - выполнять не надо.
                    if (child["geojson"]["type"].ToString() == "MultiPolygon")
                    {
                        foreach (JArray coordinate in coordinates)
                        {
                            RemoveUnnecessaryCoordinates(coordinate, frequency);
                        }
                    }
                    else if (child["geojson"]["type"].ToString() == "Polygon")
                    {
                        RemoveUnnecessaryCoordinates(coordinates, frequency);
                    }
                }
                System.IO.File.WriteAllText(@fileName, json.ToString());
            }
            else System.IO.File.WriteAllText(@fileName, dataFromRequest);
            Console.ReadKey();
            #endregion
        }

        /// <summary>Метод для удаления лишних точек из массива координат с учётом введённой частоты.        
        /// </summary>
        /// <param name="coordinates">Массив, в котором необходимо удалить лишние точки.</param>
        /// <param name="frequency">Частота точек.</param>
        /// <returns></returns>
        static void RemoveUnnecessaryCoordinates(JArray coordinates, int frequency)
        {
            int count = ((JArray)coordinates[0]).Count();
            for (int i = count - 1; i >= 0; i--)
            {
                if (i % frequency != 0)
                    ((JArray)coordinates[0]).RemoveAt(i);
            }
        }
    }
}
