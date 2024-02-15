# unity-grid-inventory

<img src="./itch.io-content/grid inventory.png" style="height: 300px" >

## Table of contents
- [Description](#description)
- [Usage](#usage)
- [License](#license)

## Description

Made with **Unity Engine 2021.3.18f1**

Play online at https://dimapepino.itch.io/grid-inventory

📢 **Description**

Grid based inventory made in **Unity Engine** using **UI Toolkit**.

Currently there are only 2 types of items:
- Default
- Backpack (can contain other items)
- However there are no restrictions about adding food item to wallet 🤔

🕹️ **How to play**
- Hold <kbd>LMB</kbd> (on item) — drag & drop
- Click <kbd>Ctrl</kbd> + <kbd>LMB</kbd> (on item) — remove item
- Click <kbd>RMB</kbd> (on item) — open backpack
- Press <kbd>R</kbd> (when drag is active) — rotate item

⭐️ **Credits**

Awesome sprites from https://innawoods.net/ (however there are no **Terms of Service** and I got no answer about external usage of their content, so I decided credit it here at least)

## Usage
- Navigate to **SampleScene** and enter playmode.
- Create custom item by using context menu **Assets/Create/SO/\***, setup fields and drag into active collection.
- If you want to build project in WebGL don't forget to remove scene **Scene Webgl Start** in **EditorBuildSettings** otherwise you would not be able to play game (I made it for [security](https://github.com/kukumberman/Unity-Webgl-Utils) reasons)

## License

This project is licensed under the MIT License, see [LICENSE.md](./LICENSE.md) for more information.
