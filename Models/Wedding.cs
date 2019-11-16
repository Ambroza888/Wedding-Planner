using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
  public class Wedding 
  {
    public int WeddingId {get;set;}
    [Required]
    public string Groom {get;set;}
    [Required]
    public string Bride {get;set;}

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date {get;set;}
    [Required]
    public string Adress {get;set;}
    public User User {get;set;}
    public int UserId {get;set;}

    public List<Rsvp> Rsvps {get;set;}
    public DateTime CreatedAt {get;set;} = DateTime.Now;
    public DateTime UpdatedAt {get;set;} = DateTime.Now;
  }
}