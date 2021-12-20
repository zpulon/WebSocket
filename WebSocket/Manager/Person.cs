using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace WebSocket.Manager
{
    //[ElasticsearchType(RelationName = "people")]
    public class Person
    {
     
        public int Id { get; set; }
        //[Text(Name = "firstname")]
        public string FirstName { get; set; }
        //[Text(Name = "lastname")]
        public string LastName { get; set; }
    }
}
