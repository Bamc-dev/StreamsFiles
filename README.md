# StreamsFilesFront

Projet front permettant de streamer en simultané un fichier vidéo (une partie client-side pour pouvoir sélectionner les sous-titres et l'audio souhaités).

## Default Idea

Projet front permettant de streamer en simultané un fichier vidéo.

- Une partie client-side permettant de sélectionner les sous-titres et l'audio souhaités.
- Sélection de sous-titres et d'audio, upload de fichier (en chunks pour éviter que le serveur HTTP n'expire), mode plein écran, fonctionnalités d'un lecteur vidéo.
- Il s'agissait d'un projet permettant de regarder du contenu sans perte de qualité avec différentes personnes à distance.

## Demo

[Voir la démo](https://github.com/user-attachments/assets/b0543d28-6f4b-4296-a836-6957c35620c2)

## How to Setup

### Prérequis

- [.NET SDK](https://dotnet.microsoft.com/download) version 8.0 ou supérieure.
- [BackEndRemoteStream](https://github.com/Bamc-dev/RemoteStreaming)

### Installation

```bash
# Clonez le dépôt
git clone https://github.com/Bamc-dev/StreamsFiles.git

# Allez dans le répertoire du projet
cd StreamsFiles

# Construisez le projet
dotnet build
```

Ensuite, il faudra aller dans les paramètres et configurer les éléments suivants :

- URL Websocket : ws://{votre-ip-de-serveur}:4532/socket
- URL API : http://{votre-ip-de-serveur}:4532

## Améliorations
Il reste beaucoup de corrections à faire. J'avais besoin de quelque chose de fonctionnel à la base, et je n'ai pas repris le projet depuis.
