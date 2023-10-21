using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;


namespace StockManager
{
    public partial class MainWindow : Window
    {
        private SQLiteConnection connection;

        public MainWindow()
        {
            InitializeComponent();

            // Create database if it doesn't exist
            if (!File.Exists("stock.db"))
            {
                SQLiteConnection.CreateFile("stock.db");
            }

            // Open connection to database
            connection = new SQLiteConnection("Data Source=stock.db;Version=3;");
            connection.Open();

            // Create table if it doesn't exist
            string createTable = "CREATE TABLE IF NOT EXISTS stock (id INTEGER PRIMARY KEY AUTOINCREMENT, modelnumber VARCHAR(100), name VARCHAR(100), color VARCHAR(100),quantity INTEGER, price REAL)";
            SQLiteCommand command = new SQLiteCommand(createTable, connection);
            command.ExecuteNonQuery();

            // Populate listbox with data from database
            string selectQuery = "SELECT * FROM stock ORDER BY modelnumber";
            command = new SQLiteCommand(selectQuery, connection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string artNumber = reader.GetString(1);
                string name = reader.GetString(2);
                string color = reader.GetString(3);
                int quantity = reader.GetInt32(4);
                double price = reader.GetDouble(5);

                StockItem item = new StockItem(artNumber, name, color, quantity, price);
                stockList.Items.Add(item);
            }
        }

        //delete one item from  the row if ammount more than one remove 1 not all
        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            StockItem item = (StockItem)stockList.SelectedItem;
            int index = stockList.SelectedIndex;

            if (item.Quantity > 1)
            {
                item.Quantity--;
                stockList.Items.RemoveAt(index);
                stockList.Items.Insert(index, item);

                string updateQuery = "UPDATE stock SET quantity = @quantity WHERE modelnumber = @modelnumber";
                SQLiteCommand command = new SQLiteCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@quantity", item.Quantity);
                command.Parameters.AddWithValue("@modelnumber", item.ArtNumber);
                command.ExecuteNonQuery();
            }
            else
            {
                stockList.Items.RemoveAt(index);

                string deleteQuery = "DELETE FROM stock WHERE modelnumber = @modelnumber";
                SQLiteCommand command = new SQLiteCommand(deleteQuery, connection);
                command.Parameters.AddWithValue("@modelnumber", item.ArtNumber);
                command.ExecuteNonQuery();
            }
        }

        //remove all items from the row 
        private void DeleteAllItems(object sender, RoutedEventArgs e)
        {
            StockItem item = (StockItem)stockList.SelectedItem;
            int index = stockList.SelectedIndex;

            stockList.Items.RemoveAt(index);

            string deleteQuery = "DELETE FROM stock WHERE modelnumber = @modelnumber";
            SQLiteCommand command = new SQLiteCommand(deleteQuery, connection);
            command.Parameters.AddWithValue("@modelnumber", item.ArtNumber);
            command.ExecuteNonQuery();
        }

        //add one item to the row
        private void AddItem(object sender, RoutedEventArgs e)
        {
            StockItem item = (StockItem)stockList.SelectedItem;
            int index = stockList.SelectedIndex;

            item.Quantity++;
            stockList.Items.RemoveAt(index);
            stockList.Items.Insert(index, item);

            string updateQuery = "UPDATE stock SET quantity = @quantity WHERE modelnumber = @modelnumber";
            SQLiteCommand command = new SQLiteCommand(updateQuery, connection);
            command.Parameters.AddWithValue("@quantity", item.Quantity);
            command.Parameters.AddWithValue("@modelnumber", item.ArtNumber);
            command.ExecuteNonQuery();
        }

        //add new item to the row
        private void AddNewItem(object sender, RoutedEventArgs e)
        {
            string artNumber;
            string name;
            string color;
            int quantity;
            double price;

            if (artNumberInput.Text == "") artNumber = "001";
            else artNumber = artNumberInput.Text;
            if (nameInput.Text == "") name = "Empty";
            else name = nameInput.Text;
            if (colorInput.Text == "") color = "Empty";
            else color = colorInput.Text;
            if (quantityInput.Text == "") quantity = 1;
            else quantity = int.Parse(quantityInput.Text);
            if (priceInput.Text == "") price = 0;
            else price = double.Parse(priceInput.Text);

            //if item already exists update quantity 
            bool itemExists = false;

            foreach (StockItem item in stockList.Items)
            {
                if (item.ArtNumber == artNumber)
                {
                    if (item.Color != color)
                    {
                        itemExists = false;
                    }

                    else
                    {
                        item.Quantity += quantity;
                        stockList.Items.Refresh();

                        string updateQuery = "UPDATE stock SET quantity = @quantity WHERE modelnumber = @modelnumber";
                        SQLiteCommand command = new SQLiteCommand(updateQuery, connection);
                        command.Parameters.AddWithValue("@quantity", item.Quantity);
                        command.Parameters.AddWithValue("@modelnumber", item.ArtNumber);
                        command.ExecuteNonQuery();

                        itemExists = true;
                    }
                }
            }

            //if item doesn't exist add new item
            if (!itemExists)
            {
                StockItem item = new StockItem(artNumber, name, color, quantity, price);

                string insertQuery = "INSERT INTO stock (modelnumber, name, color, quantity, price) VALUES (@modelnumber, @name, @color,@quantity, @price)";
                SQLiteCommand command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@modelnumber", item.ArtNumber);
                command.Parameters.AddWithValue("@name", item.Name);
                command.Parameters.AddWithValue("@color", item.Color);
                command.Parameters.AddWithValue("@quantity", item.Quantity);
                command.Parameters.AddWithValue("@price", item.Price);
                command.ExecuteNonQuery();

                stockList.Items.Clear();

                //sort database
                string selectQuery = "SELECT * FROM stock ORDER BY modelnumber";
                command = new SQLiteCommand(selectQuery, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string artNumberDB = reader.GetString(1);
                    string nameDB = reader.GetString(2);
                    string colorDB = reader.GetString(3);
                    int quantityDB = reader.GetInt32(4);
                    double priceDB = reader.GetDouble(5);

                    StockItem itemDB = new StockItem(artNumberDB, nameDB, colorDB, quantityDB, priceDB);
                    stockList.Items.Add(itemDB);
                }
            }
        }

        //search for items with the same artNumber can be any collor so more rows is possible
        private void SearchItem(object sender, RoutedEventArgs e)
        {
            string artNumber = artNumberSearch.Text;

            string selectQuery = "SELECT * FROM stock WHERE modelnumber = @modelnumber ORDER BY price";
            SQLiteCommand command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@modelnumber", artNumber);
            SQLiteDataReader reader = command.ExecuteReader();

            stockList.Items.Clear();

            while (reader.Read())
            {
                string name = reader.GetString(2);
                string color = reader.GetString(3);
                int quantity = reader.GetInt32(4);
                double price = reader.GetDouble(5);

                StockItem item = new StockItem(artNumber, name, color, quantity, price);
                stockList.Items.Add(item);
            }
        }

        private void DisplayAll(object sender, RoutedEventArgs e)
        {
            string selectQuery = "SELECT * FROM stock ORDER BY modelnumber";
            SQLiteCommand command = new SQLiteCommand(selectQuery, connection);
            SQLiteDataReader reader = command.ExecuteReader();

            stockList.Items.Clear();

            while (reader.Read())
            {
                string artNumber = reader.GetString(1);
                string name = reader.GetString(2);
                string color = reader.GetString(3);
                int quantity = reader.GetInt32(4);
                double price = reader.GetDouble(5);

                StockItem item = new StockItem(artNumber, name, color, quantity, price);
                stockList.Items.Add(item);
            }
        }

        //if CloseWindow button is clicked save database exit program
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            connection.Close();
            Environment.Exit(0);
        }
    }

    public class StockItem
    {
        public string ArtNumber { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

        public StockItem(string artNumber, string name, string color, int quantity, double price)
        {
            ArtNumber = artNumber;
            Name = name;
            Color = color;
            Quantity = quantity;
            Price = price;
        }
    }
}
