﻿using LibraryBackend.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryBackend.Entities
{
    public class Loan
    {
        public int Id { get; set; }
        public bool Returned { get; set; } = false;
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0: dd-mm-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime LoanDate { get; set; } = DateTime.Now;
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0: dd-mm-yyyy}", NullDisplayText = "No date", ApplyFormatInEditMode = true)]
        public DateTime? DevolutionDate { get; set; } = null;

        //Navigation props and FK's
        public List<BorrowedBooks> BorrowedBooks { get; set; }
        public string AccountId { get; set; }
        public string UserName { get; set; }
        public ApplicationIdentityUser Account { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
