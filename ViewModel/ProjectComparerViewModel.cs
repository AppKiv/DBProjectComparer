using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBProjectComparer.ViewModel
{
    public class ProjectComparerViewModel
    {
        public string ProjectName { get; set; }
        public IEnumerable<ProjectItemViewModel> ObjectList { get; set; }
        
        // сравниваем два файла
        public void Compare(ProjectComparerViewModel item)
        {
            // 1. получить текст по item.ObjectName из db1 и db2
            // 2. вызвать winmerge c указанными файлами

        }
    }
}
