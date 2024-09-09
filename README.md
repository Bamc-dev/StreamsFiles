# StreamsFilesFront

Projet front, permettant de streamer en simultané, un fichier vidéo (une partie client side afin de pouvoir selectionner les sous titres que l'on veut et l'audio)

## Default Idea

Projet front, permettant de streamer en simultané, un fichier vidéo

- Une partie client side afin de pouvoir selectionner les sous titres que l'on veut et l'audio
- Selection de sous titres et d'audio, upload de fichier (en chunks pour que le serveur HTTP ne Timeout pas), Fullscreen, Fonctionnalités d'un lecteur video
- C'était un projet afin de pouvoir regardé du contenu sans perte de qualités avec différentes personnes présentes a distance.

## How to Setup

Décrivez ici comment configurer le projet pour un nouvel utilisateur. Incluez des instructions détaillées afin que même quelqu'un sans beaucoup d'expérience technique puisse suivre.

### Prérequis

- [.NET SDK](https://dotnet.microsoft.com/download) Version 8.0 or more

### Installation

```bash
# Clonez le dépôt
git clone https://github.com/Bamc-dev/StreamsFiles.git

# Allez dans le répertoire du projet
cd StreamsFiles

# Build project
dotnet build
