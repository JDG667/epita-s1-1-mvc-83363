using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.MVC.Models
{
    public class Inspection
    {
        public int Id { get; set; }

        
        public int PremisesId { get; set; }
        public DateTime InspectionDate { get; set; }
        [Range(0, 100)]
        public int Score { get; set; }
        public string Outcome{ get; set; }
        public string Notes{ get; set; }


       

        // 2. La propriété de navigation (l'objet)
        // On ajoute l'attribut [ForeignKey] pour forcer la liaison
        //[ForeignKey("PremisesId")]
        public virtual Premises? Premises { get; set; }
        public virtual ICollection<FollowUp>? FollowUps { get; set; }
    }
}
