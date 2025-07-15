# AutoClicker Pro

Tehokas ja monipuolinen autoclicker Windows-ympäristöön, joka tarjoaa sekä yksinkertaisen klikkauksen automaation että kehittyneet makro-ominaisuudet.

## 🚀 Ominaisuudet

### Perus-autoclicker
- **Säädettävä nopeus**: 1-1000 klikkauksia sekunnissa
- **Hiiren painikkeet**: Vasen tai oikea klikkaus
- **Kiinteä sijainti**: Määritä tarkka paikka klikkauksille
- **Globaali F1-hotkey**: Käynnistä/pysäytä mistä tahansa sovelluksesta

### Makrojen tallennus ja toisto
- **Globaali hiirikoukku**: Tallentaa klikkaukset koko näytöltä
- **Täydellinen ajoitus**: Toistaa makrot oikeilla viiveillä
- **Reaaliaikainen näkymä**: Näe tallennetut toiminnot listassa
- **F2/F3 hotkeyt**: Nopea makrojen hallinta

### Turvallisuus ja vakaus
- **Älykäs suojaus**: Ei tallenna omien nappien klikkauksia
- **Ajoitusturva**: Estää vahingolliset aktivoinnit
- **Debug-ikkuna**: Näe mitä tapahtuu reaaliajassa
- **Turvallinen lopetus**: Puhdistaa resurssit oikein

## 📋 Vaatimukset

- Windows 10/11
- .NET 8.0 SDK
- Visual Studio Code (suositeltu) tai Visual Studio

## 🛠️ Asennus ja käyttö

### 1. Kloonaa repositorio
```bash
git clone https://github.com/[käyttäjänimi]/AutoClicker.git
cd AutoClicker
```

### 2. Käännä projekti
```bash
dotnet build
```

### 3. Aja sovellus
```bash
dotnet run
```

## 🎮 Käyttöohje

### Perus-klikkaus
1. Aseta haluamasi CPS (klikkauksia sekunnissa)
2. Valitse hiiren painike (vasen/oikea)
3. Paina **F1** tai "Käynnistä" käynnistääksesi
4. Paina **F1** uudestaan pysäyttääksesi

### Kiinteä sijainti
1. Rastita "Kiinteä sijainti"
2. Klikkaa "Aseta sijainti"
3. Odota 3 sekuntia ja siirrä hiiri haluttuun paikkaan
4. Klikkaukset kohdistuvat nyt tuohon paikkaan

### Makrojen käyttö

**Tallentaminen:**
1. Paina **F2** tai "Aloita tallennus"
2. Klikkaa haluamiasi paikkoja muissa sovelluksissa
3. Paina **F2** uudestaan lopettaaksesi tallennuksen

**Toistaminen:**
1. Paina **F3** tai "Toista makro"
2. Makro toistuu automaattisesti oikealla ajoituksella
3. Paina **F3** uudestaan pysäyttääksesi

## ⌨️ Pikanäppäimet

| Näppäin | Toiminto |
|---------|----------|
| **F1** | Käynnistä/pysäytä perus-klikkeri |
| **F2** | Aloita/lopeta makron tallennus |
| **F3** | Toista/pysäytä makro |

## 🏗️ Tekninen toteutus

- **Kieli**: C# (.NET 8.0)
- **UI**: Windows Forms
- **API**: Win32 SendInput ja SetWindowsHookEx
- **Arkkitehtuuri**: Event-driven, thread-safe

### Keskeisiä komponentteja:
- `SendInput()` - Natiivi hiiren klikkausten lähettäminen
- `SetWindowsHookEx()` - Globaali hiirikoukku makrojen tallennukseen
- `RegisterHotKey()` - Globaalit pikanäppäimet
- Timer-pohjainen tarkka ajoitus

## ⚠️ Vastuuvapauslauseke

Tämä työkalu on tarkoitettu:
- **Tuottavuuden parantamiseen** toistuvissa tehtävissä
- **Testaukseen** ja automaatioon
- **Henkilökohtaiseen käyttöön**

**Huomioi:**
- Varmista että automaatio on sallittua käyttöympäristössäsi
- Monet pelit kieltävät autoklikkereita - käytä vastuullisesti
- Noudata aina käyttöehtoja ja paikallista lainsäädäntöä

## 🤝 Osallistuminen

Projektiin voi osallistua:
1. Fork repositorio
2. Luo feature branch (`git checkout -b feature/uusi-ominaisuus`)
3. Commitoi muutokset (`git commit -am 'Lisää uusi ominaisuus'`)
4. Push branchiin (`git push origin feature/uusi-ominaisuus`)
5. Avaa Pull Request

## 📄 Lisenssi

Tämä projekti on lisensoitu MIT-lisenssillä - katso [LICENSE](LICENSE) tiedosto yksityiskohdista.

## 🔮 Tulevat ominaisuudet

- [ ] Näppäimistökomennot makroissa
- [ ] Makrojen tallennus tiedostoihin
- [ ] Satunnaiset viiveet
- [ ] Makrojen loop-toisto
- [ ] Hätäpysäytys-toiminto
- [ ] Profiilit ja asetuksien tallennus

## 📞 Tuki

Jos kohtaat ongelmia:
1. Tarkista [Issues](../../issues) sivulta olemassa olevat raportit
2. Luo uusi issue kuvaamalla ongelma tarkasti
3. Liitä debug-ikkunan viestit mukaan

---

**Kehittäjä**: [Sinun nimesi]  
**Versio**: 1.0.0  
**Viimeisin päivitys**: $(date '+%d.%m.%Y')