using System.Data;
using Microsoft.Data.SqlClient;

namespace BibliotekaKlasa.TehnoloskeKlase
{
    public abstract class TabelaKlasa
    {
        protected KonekcijaKlasa KonekcijaObjekat { get; }

        protected TabelaKlasa(KonekcijaKlasa konekcijaObjekat)
        {
            KonekcijaObjekat = konekcijaObjekat;
        }

        protected DataTable DajPodatke(
            string upit,
            params SqlParameter[] parametri)
        {
            using var konekcija = KonekcijaObjekat.KreirajOtvorenuKonekciju();
            using var komanda = new SqlCommand(upit, konekcija);

            if (parametri.Length > 0)
            {
                komanda.Parameters.AddRange(parametri);
            }

            using var adapter = new SqlDataAdapter(komanda);
            var tabela = new DataTable();
            adapter.Fill(tabela);
            return tabela;
        }

        protected int IzvrsiAzuriranje(
            string upit,
            params SqlParameter[] parametri)
        {
            using var konekcija = KonekcijaObjekat.KreirajOtvorenuKonekciju();
            using var komanda = new SqlCommand(upit, konekcija);

            if (parametri.Length > 0)
            {
                komanda.Parameters.AddRange(parametri);
            }

            return komanda.ExecuteNonQuery();
        }

        protected object? IzvrsiSkalar(
            string upit,
            params SqlParameter[] parametri)
        {
            using var konekcija = KonekcijaObjekat.KreirajOtvorenuKonekciju();
            using var komanda = new SqlCommand(upit, konekcija);

            if (parametri.Length > 0)
            {
                komanda.Parameters.AddRange(parametri);
            }

            return komanda.ExecuteScalar();
        }
    }
}
