using System;
using System.Collections.Generic;
using System.Reflection;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation;

namespace ExcelFormulaCalculation
{
    public class Stock
    {
        public string Ticker { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create a list of stock objects
            List<Stock> stocks = new List<Stock>
            {
                new Stock { Ticker = "AAPL", Price = 150.25, Quantity = 10 },
                new Stock { Ticker = "GOOGL", Price = 1050.00, Quantity = 5 },
                new Stock { Ticker = "MSFT", Price = 230.50, Quantity = 8 },
            };

            CalculateTotalValue(stocks, "Price", "Quantity");
        }

        static void CalculateTotalValue<T>(List<T> items, string pricePropertyName, string quantityPropertyName)
        {
            // Create CalcData object
            CalcData calcData = new CalcData();

            // Add items to CalcData object
            for (int i = 0; i < items.Count; i++)
            {
                T item = items[i];
                PropertyInfo priceProperty = item.GetType().GetProperty(pricePropertyName);
                PropertyInfo quantityProperty = item.GetType().GetProperty(quantityPropertyName);

                double price = Convert.ToDouble(priceProperty.GetValue(item));
                int quantity = Convert.ToInt32(quantityProperty.GetValue(item));

                calcData.SetValueRowCol(i + 1, 1, price);
                calcData.SetValueRowCol(i + 1, 2, quantity);
            }

            // Create CalcEngine object
            CalcEngine engine = new CalcEngine(calcData);

            // Define a formula to calculate the total value of each item
            string formula = "A1 * B1";

            Console.WriteLine($"{"Item", -10}\t{"Price", -10}\t{"Quantity", -10}\t{"Total Value", -10}");
            for (int i = 0; i < items.Count; i++)
            {
                // Update the formula with the current row
                string updatedFormula = formula.Replace("1", (i + 1).ToString());

                // Calculate the total value
                double totalValue = engine.ComputeValue(updatedFormula);

                T item = items[i];
                PropertyInfo priceProperty = item.GetType().GetProperty(pricePropertyName);
                PropertyInfo quantityProperty = item.GetType().GetProperty(quantityPropertyName);

                double price = Convert.ToDouble(priceProperty.GetValue(item));
                int quantity = Convert.ToInt32(quantityProperty.GetValue(item));

                Console.WriteLine($"{(i + 1), -10}\t{price, -10}\t{quantity, -10}\t{totalValue, -10}");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation;

namespace ExcelFormulaCalculation
{
    public class Stock
    {
        public string Ticker { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create a list of stock objects
            List<Stock> stocks = new List<Stock>
            {
                new Stock { Ticker = "AAPL", Price = 150.25, Quantity = 10 },
                new Stock { Ticker = "GOOGL", Price = 1050.00, Quantity = 5 },
                new Stock { Ticker = "MSFT", Price = 230.50, Quantity = 8 },
            };

            // Create CalcData object
            CalcData calcData = new CalcData();

            // Add stocks to CalcData object
            for (int i = 0; i < stocks.Count; i++)
            {
                calcData.SetValueRowCol(i + 1, 1, stocks[i].Ticker);
                calcData.SetValueRowCol(i + 1, 2, stocks[i].Price);
                calcData.SetValueRowCol(i + 1, 3, stocks[i].Quantity);
            }

            // Create CalcEngine object
            CalcEngine engine = new CalcEngine(calcData);

            // Define a formula to calculate the total value of each stock
            string formula = "B1 * C1";

            Console.WriteLine("Ticker\tPrice\tQuantity\tTotal Value");
            for (int i = 0; i < stocks.Count; i++)
            {
                // Update the formula with the current row
                string updatedFormula = formula.Replace("1", (i + 1).ToString());

                // Calculate the total value of the stock
                double totalValue = engine.ComputeValue(updatedFormula);

                Console.WriteLine($"{stocks[i].Ticker}\t{stocks[i].Price}\t{stocks[i].Quantity}\t{totalValue}");
            }
        }
    }
}
