using DotCoolControls.Global;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace DotCoolControls.SampleApp.Data
{
    [DataContract]
    public class CustomerTable
    {
        [DataMember(Name = "Customers")]
        public List<Customer> Customers { get; protected set; }
    }

    [DataContract]
    public class Customer
    {
        [DataMember]
        public int CustomerID { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Phone { get; set; }

        [DataMember]
        public DateTime JoinDate { get; set; }
    }

    [DataContract]
    public class OrderTable
    {
        [DataMember(Name = "Orders")]
        public List<Order> Orders { get; protected set; }
    }

    [DataContract]
    public class Order
    {
        [DataMember]
        public int OrderID { get; set; }

        [DataMember]
        public int CustomerID { get; set; }

        [DataMember]
        public string InvoiceNo { get; set; }

        [DataMember]
        public DateTime OrderDate { get; set; }
    }

    [DataContract]
    public class OrderProductTable
    {
        [DataMember(Name = "OrderProducts")]
        public List<OrderProduct> OrderProducts { get; protected set; }
    }

    [DataContract]
    public class OrderProduct
    {
        [DataMember]
        public int OrderID { get; set; }

        [DataMember]
        public int iProductID { get; set; }

        [DataMember]
        public int iQuantity { get; set; }
    }

    [DataContract]
    public class ProductTable
    {
        [DataMember(Name = "Products")]
        public List<Product> Products { get; protected set; }
    }

    [DataContract]
    public class Product
    {        
        [DataMember]
        public int ProductID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ProductCode { get; set; }

        [DataMember]
        public decimal Price { get; set; }
    }

    public static class SampleDataConvert
    {
        public static void ConvertToDataTable<T>(ref DataTable dtTable, IList<T> data)
        {
            try
            {
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

                if (dtTable == null)
                {
                    dtTable = new DataTable();

                    for (int i = 0; i < props.Count; i++)
                    {
                        PropertyDescriptor prop = props[i];
                        dtTable.Columns.Add(prop.Name, prop.PropertyType);
                    }
                }//end if

                object[] values = new object[props.Count];
                foreach (T item in data)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = props[i].GetValue(item);
                    }
                    dtTable.Rows.Add(values);
                }
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ConvertToDataTable function of SampleDataConvert class.");
            }
        }

        
        /* NOT USED
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
            TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
        */
    }    
}
