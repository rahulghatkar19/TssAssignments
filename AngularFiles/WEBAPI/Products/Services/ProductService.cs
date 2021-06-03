using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json;
using Products.Model;
using Microsoft.EntityFrameworkCore;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;


namespace Products.Services
{
    public class ProductService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ProductContext _context;
        public bool _paymentStatus;

        public ProductService(IHttpClientFactory clientFactory, ProductContext context)
        {
            _clientFactory = clientFactory;
            _context = context;
        }

        public long AddProduct(ProductDB obj)
        {
            var newProduct = new ProductDB()
            {
                ProductName = obj.ProductName,
                ProductDesc = obj.ProductDesc,
                ProductavailableQuantity=obj.ProductavailableQuantity
            };

            //try 
            //{
            //    _context.Products.Add(newProduct);
            //    _context.SaveChanges();
            //}
            //catch(Exception e)
            //{
            //    return e.ToString();
            //}
            //return "Success";
            _context.Products.Add(newProduct);
            _context.SaveChanges();

            return newProduct.ProductId;
        }

        public async Task<List<ProductDB>> GetAllProducts()
        {
            var products = new List<ProductDB>();
            var tableData = await _context.Products.ToListAsync();
            if(tableData.Any()==true)//data present in table
            {
                foreach(var product in tableData)
                {
                    products.Add(new ProductDB() 
                    {
                        ProductId=product.ProductId,
                        ProductName=product.ProductName,
                        ProductDesc=product.ProductDesc,
                        ProductavailableQuantity=product.ProductavailableQuantity
                    });
                }
            }
            Debug.WriteLine(products[0]);
            Debug.WriteLine(products[0].ProductName);

            return products;
        }


        List<string> msg = new List<string>();
        int[] added = { 0, 0, 0, 0 };


        public async Task<List<string>> checkQuantity(addedProduct[] addedToCart)
        {
            List<ProductDB> products = new List<ProductDB>();
            products = await GetAllProducts();

            //Products.ForEach(x => avl.Add(x.product_availableQuantity));

            for (int i = 0; i < addedToCart.Length; i++)
            {
                /*List<ProductDB> products = new List<ProductDB>();
                products = await GetAllProducts();*/
                var k = products.FindIndex(x => x.ProductId == addedToCart[i].productId);

                Debug.WriteLine(k);
                if(addedToCart[i].productaddedQuantity > products[k].ProductavailableQuantity)
                {
                    Debug.WriteLine("Insufficient");
                    msg.Add("Insufficient");
                    msg.Add("Order Incomplete");
                    msg.Add("Order could not be completed as we have Insufficient Quantity of Items required. Sorry!");
                    msg.Add("View Cart");
                    return msg;
                }
                else
                {
                    var updateSql = _context.Products.SingleOrDefault(p => p.ProductId == addedToCart[i].productId);
                    if (updateSql != null)
                    {
                        updateSql.ProductavailableQuantity = updateSql.ProductavailableQuantity - addedToCart[i].productaddedQuantity;
                    }
                }
            }

            var client = _clientFactory.CreateClient();
            var response = await client.GetStringAsync("https://localhost:44356/payment");
            bool paymentStatus = JsonConvert.DeserializeObject<bool>(response);

            if (paymentStatus)
            {
                _context.SaveChanges();//saved a loop

                var removeProduct = from p in _context.Products
                                 where p.ProductavailableQuantity == 0
                                 select p;

                foreach (var p in removeProduct.ToList())//can this cause performance issues in a huge data set
                {
                    _context.Products.Remove(p);
                    _context.SaveChanges();
                }


                Debug.WriteLine("Payment Succ");
                msg.Add("Success");
                msg.Add("Thank You!");
                msg.Add("Payment Successful.");
                msg.Add("Continue Shopping");
                return msg;
            }
            else
            {
                Debug.WriteLine("Payment Fail");
                msg.Add("Failure");
                msg.Add("Order Incomplete");
                msg.Add("Order could not be completed as your Payment Failed. Please Try Again.");
                msg.Add("View Cart");
                return msg;
            }
        }

        public string[] setMessage(string[] message)
        {
            return message;
        }

        public void addQuantity(int pid)
        {
            added[pid] = 1;
        }

        public int[] getQuantity()
        {
            return added;
        }

        public async void paymentHttpCall()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetStringAsync("https://localhost:44356/payment");
            bool extract = JsonConvert.DeserializeObject<bool>(response);
            Debug.WriteLine(response);
            Debug.WriteLine(extract);
            _paymentStatus = extract;
        }
    }
}

//List<product> Products = new List<product>()
//{
//    new product { product_id=0, product_name = "Product 1", product_desc = "P1", product_availableQuantity = 24 },
//    new product { product_id=1, product_name = "Product 2", product_desc = "P2", product_availableQuantity = 34 },
//    new product { product_id=2, product_name = "Product 3", product_desc = "P3", product_availableQuantity = 34 },
//    new product { product_id=3, product_name = "Product 4", product_desc = "P4", product_availableQuantity = 34 },
//};

//List<int> avl = new List<int>();

//public List<product> getProducts()
//{
//    return Products;
//}