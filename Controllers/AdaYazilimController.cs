using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdaYazilim.Controllers
{
    public class yerlesimAyrinti
    {
        public string VagonAdi { get; set; }
        public int KisiSayisi { get; set; }
    }

    public class AdaYazilimController : Controller
    {

        [HttpPost]
        public dynamic TryReservation([FromBody] dynamic input)
        {
            dynamic jsonForm = JsonConvert.DeserializeObject(input.ToString());

            double numberOfReservation = jsonForm.RezervasyonYapilacakKisiSayisi;
            var vagons = jsonForm.Tren.Vagonlar;
            bool differentVagon = jsonForm.KisilerFarkliVagonlaraYerlestirilebilir;
            bool reservation = false;

            List<yerlesimAyrinti> yerlesim = new List<yerlesimAyrinti>();
            List<yerlesimAyrinti> yerlesimMultiple = new List<yerlesimAyrinti>();

            var result = new
            {
                RezervasyonYapilabilir = false,
                YerlesimAyrinti = yerlesim
            };

            //check only one vagon 
            if (differentVagon == false)
            {
                foreach (var vagon in vagons)
                {
                    double Kapasite = vagon.Kapasite;
                    double DoluKoltukAdet = vagon.DoluKoltukAdet;
                    double controlKoltuk = Kapasite * 0.7;

                    if (DoluKoltukAdet < controlKoltuk)
                    {

                        //check the capacity limit of the vagon
                        if ((Kapasite - DoluKoltukAdet) >= numberOfReservation)
                        {
                            reservation = true;
                            yerlesimAyrinti yerlesenler = new yerlesimAyrinti();

                            yerlesenler.VagonAdi = vagon.Ad;
                            yerlesenler.KisiSayisi = (int)numberOfReservation;

                            yerlesim.Add(yerlesenler);
                        }

                        break;
                    }
                };

                var result2 = new
                {
                    RezervasyonYapilabilir = reservation,
                    YerlesimAyrinti = yerlesim
                };

                result = result2;

            }
            //check all vagon 
            else if (differentVagon == true)
            {
                foreach (var vagon in vagons)
                {
                    double Kapasite = vagon.Kapasite;
                    double DoluKoltukAdet = vagon.DoluKoltukAdet;
                    double controlKoltuk = Kapasite * 0.7;
                    //passengers can have a seat
                    if (DoluKoltukAdet < controlKoltuk)
                    {
                        double numberOfFreeSeat = Kapasite - DoluKoltukAdet;
                        //find the number of people who could not find a seat on the vagon
                        numberOfReservation = numberOfReservation - numberOfFreeSeat;
                        //if the number is less or equal to 0, all of them  have found a seat otherwise look for the other vagon
                        if (numberOfReservation > 0)
                        {
                            yerlesimAyrinti yerlesenler = new yerlesimAyrinti();
                            yerlesenler.VagonAdi = vagon.Ad;
                            yerlesenler.KisiSayisi = (int)numberOfFreeSeat;
                            yerlesimMultiple.Add(yerlesenler);
                        }
                        else if (numberOfReservation <= 0)
                        {
                            reservation = true;
                            yerlesimAyrinti yerlesenler = new yerlesimAyrinti();
                            yerlesenler.VagonAdi = vagon.Ad;
                            yerlesenler.KisiSayisi = (int)numberOfReservation + (int)numberOfFreeSeat;
                            yerlesimMultiple.Add(yerlesenler);
                            break;
                        }

                    }
                };

                if (reservation == true)
                {
                    yerlesim = yerlesimMultiple;
                }

                var result2 = new
                {
                    RezervasyonYapilabilir = reservation,
                    YerlesimAyrinti = yerlesim
                };
                result = result2;
            }
            return result;
        }
    }
}
