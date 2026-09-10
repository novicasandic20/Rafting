using Microsoft.Data.SqlClient;

namespace BibliotekaKlasa.TehnoloskeKlase
{
    public sealed class TransakcijaKlasa : IDisposable
    {
        public SqlConnection Konekcija { get; }
        public SqlTransaction Transakcija { get; }
        private bool _zavrsena;

        public TransakcijaKlasa(KonekcijaKlasa konekcijaObjekat)
        {
            Konekcija = konekcijaObjekat.KreirajOtvorenuKonekciju();
            Transakcija = Konekcija.BeginTransaction();
        }

        public void Potvrdi()
        {
            Transakcija.Commit();
            _zavrsena = true;
        }

        public void Ponisti()
        {
            if (!_zavrsena)
            {
                Transakcija.Rollback();
                _zavrsena = true;
            }
        }

        public void Dispose()
        {
            if (!_zavrsena)
            {
                Transakcija.Rollback();
            }

            Transakcija.Dispose();
            Konekcija.Dispose();
        }
    }
}
