using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerTecnomer;

namespace TaskManagerTecnomer.Models
{
    public class TaskModels
    {
        public int Id { get; set; }//айдишник
        public string? Title { get; set; }//название
        public string? Description { get; set; }//описание
        public int Status { get; set; }//статус
        public DateTime datecreation { get; set; }//дата создания


    }
}
