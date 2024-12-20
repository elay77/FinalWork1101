using DatabaseLibrary.Models;
using DatabaseLibrary.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FragrantWorld.Pages
{
    /// <summary>
    /// Логика взаимодействия для ShopPage.xaml
    /// </summary>
    public partial class ShopPage : Page
    {
        private readonly ProductService _productService = new();
        private readonly UserService _userService = new();
        private List<Product> _allProducts = new();
        private List<Product> _filteredProducts = new();

        public ShopPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProductsAsync();

            // Заполнение списка производителей
            var manufacturers = _allProducts.Select(p => p.Manufacturer).Distinct().OrderBy(m => m).ToList();
            manufacturers.Insert(0, "Все производители");
            ManufacturerComboBox.ItemsSource = manufacturers;
            ManufacturerComboBox.SelectedIndex = 0;
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                _allProducts = await _productService.GetProductsAsync();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ApplyFilters()
        {
            var query = _allProducts.AsEnumerable();

            // Фильтр по производителю
            if (ManufacturerComboBox.SelectedIndex > 0)
            {
                string selectedManufacturer = ManufacturerComboBox.SelectedItem.ToString();
                query = query.Where(p => p.Manufacturer == selectedManufacturer);
            }

            // Фильтр по цене
            if (decimal.TryParse(MinPriceBox.Text, out decimal minPrice))
                query = query.Where(p => p.Cost >= minPrice);

            if (decimal.TryParse(MaxPriceBox.Text, out decimal maxPrice))
                query = query.Where(p => p.Cost <= maxPrice);

            // Поиск по названию
            string searchQuery = SearchTextBox.Text.ToLower();
            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(p => p.Name.ToLower().Contains(searchQuery));

            // Сортировка
            if (SortComboBox.SelectedIndex == 0)
                query = query.OrderBy(p => p.Cost);
            else if (SortComboBox.SelectedIndex == 1)
                query = query.OrderByDescending(p => p.Cost);

            _filteredProducts = query.ToList();
            UpdateProductGrid();
            UpdateProductCount();
        }

        private void UpdateProductGrid()
        {
            ProductItemsControl.ItemsSource = _filteredProducts;
        }

        private void UpdateProductCount()
        {
            ProductCountDisplay.Text = $"{_filteredProducts.Count} из {_allProducts.Count}";
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void ManufacturerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void MinPriceBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void MaxPriceBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void CreateProductConteiner(Product productItem)
        {
            try
            {
                StackPanel panel = new()
                {
                    Width = 630,
                    Margin = new Thickness(15),
                    Background = new SolidColorBrush(Color.FromRgb(255, 204, 153)),

                };

                Border topBorder = new()
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5)
                };
                Border botBorder = new()
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5)
                };

                Grid grid = new() { };
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                TextBlock ProductTextBlock = new TextBlock()
                {
                    Text = productItem.Name,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetRow(ProductTextBlock, 0);
                Grid.SetColumn(ProductTextBlock, 0);
                grid.Children.Add(ProductTextBlock);

                TextBlock DescriptionTextBlock = new TextBlock
                {
                    Text = productItem.Description
                };
                Grid.SetRow(DescriptionTextBlock, 1);
                Grid.SetColumn(DescriptionTextBlock, 0);
                grid.Children.Add(DescriptionTextBlock);

                TextBlock ManufacturerTextBlock = new TextBlock
                {
                    Text = $"Производитель: {productItem.Manufacturer}",
                };
                Grid.SetRow(ManufacturerTextBlock, 2);
                Grid.SetColumn(ManufacturerTextBlock, 0);
                grid.Children.Add(ManufacturerTextBlock);

                TextBlock PriceTextBlock = new TextBlock
                {
                    Text = $"Цена: {productItem.Cost}",
                };
                Grid.SetRow(PriceTextBlock, 3);
                Grid.SetColumn(PriceTextBlock, 0);
                grid.Children.Add(PriceTextBlock);

                Button OrderButton = new Button
                {
                    Content = "Заказать",
                    HorizontalAlignment = HorizontalAlignment.Right,
                };
                Grid.SetRow(OrderButton, 3);
                Grid.SetColumn(OrderButton, 1);
                grid.Children.Add(OrderButton);

                panel.Children.Add(topBorder);
                panel.Children.Add(grid);
                panel.Children.Add(botBorder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private TextBlock CreateTextBlock(string text, FontWeight fontWeight, int row, int column)
        {
            var textBlock = new TextBlock { Text = text, FontWeight = fontWeight };
            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, column);
            return textBlock;
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentFrame.CanGoBack)
                App.CurrentFrame.GoBack();
        }
    }
}
