(function () {

    function elementeUcesnika() {
        return Array.from(
            document.querySelectorAll(
                "#ucesniciContainer .ucesnik-card"
            )
        );
    }


    function azurirajIndekse() {
        const kartice = elementeUcesnika();

        kartice.forEach(function (kartica, indeks) {

            kartica.dataset.index = indeks;

            const naslov =
                kartica.querySelector(".ucesnik-naslov");

            if (naslov) {
                naslov.textContent =
                    "Učesnik " + (indeks + 1);
            }

            kartica
                .querySelectorAll("[data-field]")
                .forEach(function (polje) {

                    const nazivPolja =
                        polje.getAttribute("data-field");

                    polje.name =
                        `Ucesnici[${indeks}].${nazivPolja}`;
                });

            const redniBroj =
                kartica.querySelector(
                    "[data-field='RedniBroj']"
                );

            if (redniBroj) {
                redniBroj.value = indeks + 1;
            }
        });


        const broj =
            document.getElementById(
                "brojUcesnikaPrikaz"
            );

        if (broj) {
            broj.textContent = kartice.length;
        }
    }


    function podesiPorukuIdentifikacije(kartica) {

        const checkbox =
            kartica.querySelector(
                "[data-field='PotvrdioDonosenjeIdentifikacionogDokumenta']"
            );

        const poruka =
            kartica.querySelector(
                ".identifikacija-poruka"
            );

        if (!checkbox || !poruka) {
            return;
        }

        poruka.classList.toggle(
            "d-none",
            !checkbox.checked
        );
    }


    function dodajUcesnika() {

        const template =
            document.getElementById(
                "ucesnikTemplate"
            );

        const container =
            document.getElementById(
                "ucesniciContainer"
            );

        if (!template || !container) {
            return;
        }

        container.appendChild(
            template.content.cloneNode(true)
        );

        azurirajIndekse();


        const kartice =
            elementeUcesnika();

        const novaKartica =
            kartice[kartice.length - 1];

        if (novaKartica) {
            podesiPorukuIdentifikacije(
                novaKartica
            );
        }
    }


    function obrisiUcesnika(dugme) {

        const kartica =
            dugme.closest(".ucesnik-card");

        if (kartica) {
            kartica.remove();
        }

        azurirajIndekse();
    }


    function prikaziTuru() {

        const select =
            document.getElementById(
                "RaftingTuraId"
            );

        const panel =
            document.getElementById(
                "turaPreview"
            );

        if (!select || !panel) {
            return;
        }


        const opcija =
            select.options[
            select.selectedIndex
            ];


        if (!opcija || !opcija.value) {

            panel.classList.add(
                "d-none"
            );

            return;
        }


        panel.classList.remove(
            "d-none"
        );


        const mapiranje = {

            turaNaziv:
                opcija.dataset.naziv,

            turaDatum:
                opcija.dataset.datum,

            turaLokacija:
                opcija.dataset.lokacija,

            turaTezina:
                opcija.dataset.tezina,

            turaKapacitet:
                opcija.dataset.kapacitet,

            turaPreostalo:
                opcija.dataset.preostalo,

            turaCena:
                opcija.dataset.cena
        };


        Object.keys(mapiranje)
            .forEach(function (id) {

                const element =
                    document.getElementById(id);

                if (element) {
                    element.textContent =
                        mapiranje[id] || "—";
                }

            });
    }


    function proveriFormu(event) {

        const kartice =
            elementeUcesnika();

        const greske = [];

        const jmbgSkup =
            new Set();


        if (kartice.length === 0) {

            greske.push(
                "Dodajte najmanje jednog učesnika."
            );
        }


        const tura =
            document.getElementById(
                "RaftingTuraId"
            );

        if (tura && !tura.value) {

            greske.push(
                "Izaberite rafting turu."
            );
        }


        kartice.forEach(
            function (kartica, indeks) {

                const prefiks =
                    "Učesnik " +
                    (indeks + 1) +
                    ": ";


                /* =========================
                   JMBG
                   ========================= */

                const jmbg =
                    kartica.querySelector(
                        "[data-field='JMBG']"
                    )?.value.trim() || "";


                if (!/^\d{13}$/.test(jmbg)) {

                    greske.push(
                        prefiks +
                        "JMBG mora imati tačno 13 cifara."
                    );

                }
                else if (
                    jmbgSkup.has(jmbg)
                ) {

                    greske.push(
                        prefiks +
                        "JMBG je unet više puta."
                    );

                }
                else {

                    jmbgSkup.add(jmbg);
                }


                /* =========================
                   EMAIL
                   ========================= */

                const email =
                    kartica.querySelector(
                        "[data-field='Email']"
                    )?.value.trim() || "";


                if (
                    !/^[^\s@]+@[^\s@]+\.[^\s@]+$/
                        .test(email)
                ) {

                    greske.push(
                        prefiks +
                        "e-mail adresa nije ispravna."
                    );
                }


                /* =========================
                   TELEFON
                   ========================= */

                const telefon =
                    kartica.querySelector(
                        "[data-field='KontaktTelefon']"
                    )?.value.trim() || "";


                if (
                    !/^[0-9+()\/\-\s]{6,30}$/
                        .test(telefon)
                ) {

                    greske.push(
                        prefiks +
                        "kontakt telefon nije ispravan."
                    );
                }


                /* =========================
                   IDENTIFIKACIONI DOKUMENT
                   ========================= */

                const potvrdioDokument =
                    kartica.querySelector(
                        "[data-field='PotvrdioDonosenjeIdentifikacionogDokumenta']"
                    )?.checked;


                if (!potvrdioDokument) {

                    greske.push(
                        prefiks +
                        "potrebno je potvrditi donošenje lične karte ili pasoša."
                    );
                }


                /* =========================
                   DATUM ROĐENJA
                   ========================= */

                const datumTekst =
                    kartica.querySelector(
                        "[data-field='DatumRodjenja']"
                    )?.value || "";


                if (!datumTekst) {

                    greske.push(
                        prefiks +
                        "datum rođenja je obavezan."
                    );

                }
                else {

                    const datum =
                        new Date(
                            datumTekst +
                            "T00:00:00"
                        );

                    const danas =
                        new Date();

                    danas.setHours(
                        0,
                        0,
                        0,
                        0
                    );


                    if (datum > danas) {

                        greske.push(
                            prefiks +
                            "datum rođenja ne može biti u budućnosti."
                        );
                    }


                    const granica =
                        new Date(
                            danas.getFullYear() - 18,
                            danas.getMonth(),
                            danas.getDate()
                        );


                    const maloletan =
                        datum > granica;


                    /* =========================
                       SAGLASNOST
                       ========================= */

                    if (maloletan) {

                        const saglasnostCheckbox =
                            kartica.querySelector(
                                "[data-field='SaglasnostRoditeljaStaratelja']"
                            );

                        const saglasnostFajl =
                            kartica.querySelector(
                                "[data-field='SaglasnostFajl']"
                            );


                        const postojecaSaglasnost =
                            kartica.querySelector(
                                "[data-field='PostojecaSaglasnostSacuvaniNaziv']"
                            );


                        const imaSaglasnost =
                            (saglasnostCheckbox?.checked ?? false) ||
                            (saglasnostFajl?.files?.length > 0) ||
                            (
                                postojecaSaglasnost &&
                                postojecaSaglasnost.value.trim() !== ""
                            );


                        if (!imaSaglasnost) {

                            greske.push(
                                prefiks +
                                "za maloletnog učesnika potrebna je saglasnost roditelja/staratelja."
                            );
                        }
                    }
                }

            });


        const prikaz =
            document.getElementById(
                "jsGreske"
            );


        if (greske.length > 0) {

            event.preventDefault();


            if (prikaz) {

                prikaz.classList.remove(
                    "d-none"
                );

                prikaz.innerHTML =
                    "<strong>Proverite unos:</strong>" +
                    "<ul class='mb-0 mt-2'>" +

                    greske
                        .map(function (greska) {
                            return (
                                "<li>" +
                                greska +
                                "</li>"
                            );
                        })
                        .join("") +

                    "</ul>";
            }


            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });


            return false;
        }


        if (prikaz) {

            prikaz.classList.add(
                "d-none"
            );

            prikaz.innerHTML = "";
        }


        return true;
    }


    document.addEventListener(
        "DOMContentLoaded",
        function () {

            const container =
                document.getElementById(
                    "ucesniciContainer"
                );


            if (!container) {
                return;
            }


            document
                .getElementById(
                    "dodajUcesnikaDugme"
                )
                ?.addEventListener(
                    "click",
                    dodajUcesnika
                );


            container.addEventListener(
                "click",
                function (event) {

                    const dugme =
                        event.target.closest(
                            ".obrisi-ucesnika"
                        );

                    if (dugme) {

                        obrisiUcesnika(
                            dugme
                        );
                    }

                });


            container.addEventListener(
                "change",
                function (event) {

                    if (
                        event.target.matches(
                            "[data-field='PotvrdioDonosenjeIdentifikacionogDokumenta']"
                        )
                    ) {

                        const kartica =
                            event.target.closest(
                                ".ucesnik-card"
                            );

                        if (kartica) {

                            podesiPorukuIdentifikacije(
                                kartica
                            );
                        }
                    }

                });


            document
                .getElementById(
                    "RaftingTuraId"
                )
                ?.addEventListener(
                    "change",
                    prikaziTuru
                );


            document
                .getElementById(
                    "prijavaForma"
                )
                ?.addEventListener(
                    "submit",
                    proveriFormu
                );


            if (
                elementeUcesnika()
                    .length === 0
            ) {

                dodajUcesnika();

            }
            else {

                azurirajIndekse();

                elementeUcesnika()
                    .forEach(
                        podesiPorukuIdentifikacije
                    );
            }


            prikaziTuru();

        });

})();