## [6.13.0-beta.7](https://github.com/Belphemur/SoundSwitch/compare/v6.13.0-beta.6...v6.13.0-beta.7) (2025-03-20)

### Bug Fixes

* **banner:** add WS_EX_NOACTIVATE style to prevent focus stealing ([bf31371](https://github.com/Belphemur/SoundSwitch/commit/bf3137148c22f5a0707204bc5dac9d119ee2263a))
* **banner:** implement double buffering to reduce flickering and improve click handling ([0cdb808](https://github.com/Belphemur/SoundSwitch/commit/0cdb808cb2485970a33b3bdad6b8b52ae70e9331))
* **installer:** Don't recreate desktop icon when VerySilent (from auto updater). ([00cc3b3](https://github.com/Belphemur/SoundSwitch/commit/00cc3b3d4e72d419a33b26b1a9048c03ff23341f)), closes [#1720](https://github.com/Belphemur/SoundSwitch/issues/1720)
* **notification::mute:** mute microphone on clicking the banner when the microphone is On ([bfe5ece](https://github.com/Belphemur/SoundSwitch/commit/bfe5ece253ad43d68de04f4d30e00bb1c41ff021))
* **notification::mute:** reduce microphone mute banner display time to 1.5 seconds ([e82ccc4](https://github.com/Belphemur/SoundSwitch/commit/e82ccc4cfc1df2785993a6a922cdd798e294bfa7))
* **Settings:** Ensure persistent mute notification checkbox visibility is correctly set ([96eb04b](https://github.com/Belphemur/SoundSwitch/commit/96eb04ba41aac1981a365029d2908c38e1ee4df3))

### Features

* **banner:** hide banner when clicked on ([3601270](https://github.com/Belphemur/SoundSwitch/commit/36012705b7788075d379072c5abcf1b8aec61023))
* **Notification:** Add persistent mute notification configuration and update related logic ([3cecbf4](https://github.com/Belphemur/SoundSwitch/commit/3cecbf4af35f752be257582ef7d83caac1b2db4c))
* **Settings:** Let the user choose what type of notification they have for mute ([a1e181f](https://github.com/Belphemur/SoundSwitch/commit/a1e181f698ba56fc5679bede167f6a5a1a58371b))

## [6.13.0-beta.6](https://github.com/Belphemur/SoundSwitch/compare/v6.13.0-beta.5...v6.13.0-beta.6) (2025-03-14)

### Features

* **mute:** add a compact banner for the microphone mute state ([48f0aa4](https://github.com/Belphemur/SoundSwitch/commit/48f0aa40ea8859df7e6fc7fa5226701b9655b403))
* **mute:** Only show friendly name for the device for mute notification ([09eeffb](https://github.com/Belphemur/SoundSwitch/commit/09eeffb32cab571c5fe4d7a72b399af474144ffd))
* **notification::mute:** Support clicking the banner to unmute. ([67e2b3f](https://github.com/Belphemur/SoundSwitch/commit/67e2b3fe1630294de949520455d19656b2cd0d56))
* **notification::mute:** Use a compact notification for Mute that stays on the screen until the microphone is unmuted. ([7439388](https://github.com/Belphemur/SoundSwitch/commit/7439388e9d34c37e2248f9f87065c1b83d7821dd))
* **resources:** add microphone and mute images ([b221ab3](https://github.com/Belphemur/SoundSwitch/commit/b221ab3bada5c4703e91b815826e35897bfe0fe8))

## [6.13.0-beta.5](https://github.com/Belphemur/SoundSwitch/compare/v6.13.0-beta.4...v6.13.0-beta.5) (2025-03-12)

### Languages

* **tamil:** Add tamil language to the application ([31a2f04](https://github.com/Belphemur/SoundSwitch/commit/31a2f04fa8fe2735a3502637a67125060843e9ca))
* **Tamil:** Added About translation using Weblate ([7b16918](https://github.com/Belphemur/SoundSwitch/commit/7b1691803c1226a6354944a202500cb94b7930e2))
* **Tamil:** Added Settings translation using Weblate ([fc63fb5](https://github.com/Belphemur/SoundSwitch/commit/fc63fb5cff5145bd1be6066f9ff8ff3b88e23f7a))
* **Tamil:** Added Tray Icon translation using Weblate ([4b6b4df](https://github.com/Belphemur/SoundSwitch/commit/4b6b4dfc8b42e47f7c886f8066bac2fb27025e73))
* **Tamil:** Added Update Download translation using Weblate ([c8ee970](https://github.com/Belphemur/SoundSwitch/commit/c8ee970be58c7caadc3a9956352e02cf6f14690e))
* **Tamil:** Translated About using Weblate ([15ac998](https://github.com/Belphemur/SoundSwitch/commit/15ac998a35194f1686447c8177dc31bbed8d9e4f))
* **Tamil:** Translated Settings using Weblate ([e5fa0d8](https://github.com/Belphemur/SoundSwitch/commit/e5fa0d84645023da085b4bae0da7779aa9483891))
* **Tamil:** Translated Tray Icon using Weblate ([2dd3920](https://github.com/Belphemur/SoundSwitch/commit/2dd3920feb443274a9810050dc9edf5cc7d87e79))
* **Tamil:** Translated Update Download using Weblate ([afc7c76](https://github.com/Belphemur/SoundSwitch/commit/afc7c76ae423f9fdea73ad736a847ff7bd66b892))

### Bug Fixes

* **cli:mute:** notify on microphone muted state ([86e5843](https://github.com/Belphemur/SoundSwitch/commit/86e5843768a584496e833b5cd3d38f22c0c04942))
* **localization:** correct formatting in SettingsStrings.ta.resx and improve bracket checking in check_brackets.py ([365c3b8](https://github.com/Belphemur/SoundSwitch/commit/365c3b8f7d1a700b459685c422dc40a55e07c0b1))
* **localization:** improve formatting and correct placeholder in UpdateDownloadStrings.ta.resx ([74e0393](https://github.com/Belphemur/SoundSwitch/commit/74e03935dd30876b80b207d80043c2166da6b1c6))

### Features

* **audio:** add event for volume and mute state changes ([f7013a2](https://github.com/Belphemur/SoundSwitch/commit/f7013a258f67d4fbfb0de7342ed389ad4a4d860f))
* **device:** track if the the device is muted. ([5b506d7](https://github.com/Belphemur/SoundSwitch/commit/5b506d786a36813a69e15a7181c8f40953ec9c23))
* **microphone:** Notify on any microphone being muted. ([b464e2d](https://github.com/Belphemur/SoundSwitch/commit/b464e2dd0de50b5711f9a056eb7989aa2d15c3e5))

## [6.13.0-beta.4](https://github.com/Belphemur/SoundSwitch/compare/v6.13.0-beta.3...v6.13.0-beta.4) (2025-03-03)

### Bug Fixes

* **update:** fix update ui crashing. ([70ba6c1](https://github.com/Belphemur/SoundSwitch/commit/70ba6c1d7015768aa3d775239ad45bc98d9f9b16)), closes [#1693](https://github.com/Belphemur/SoundSwitch/issues/1693)

## [6.13.0-beta.3](https://github.com/Belphemur/SoundSwitch/compare/v6.13.0-beta.2...v6.13.0-beta.3) (2025-03-01)

### Enhancements

* **hotkey:** accept alone keys for hot keys like PrintScr, Pause, Home, End, etc ... and any function key alone too. ([65c7863](https://github.com/Belphemur/SoundSwitch/commit/65c7863006dee941c57cfc416351b4e05a06e928))
* **hotkey:** Change the display of hotkeys modifier to always follow ctrl, shift, alt, win. ([b518688](https://github.com/Belphemur/SoundSwitch/commit/b5186887ad536a4dc208b84c9ac854a75510631d))
* **hotkey:** support setting only one key for hotkey ([c9e8131](https://github.com/Belphemur/SoundSwitch/commit/c9e81319fd17104272fcacb4e7ae987bf682ccbe))

### Languages

* **Chinese (Traditional Han script):** Translated Settings using Weblate ([05b085b](https://github.com/Belphemur/SoundSwitch/commit/05b085b35a6fc986c90ca14f9aaff1c07ebc117b))
* **Chinese (Traditional Han script):** Translated Tray Icon using Weblate ([986cfa8](https://github.com/Belphemur/SoundSwitch/commit/986cfa808802d0d9ef29ddf615fe4c12534f4718))
* **Chinese (Traditional Han script):** Translated Update Download using Weblate ([dbf1303](https://github.com/Belphemur/SoundSwitch/commit/dbf13034418c9362a7b65f972ba37fab8dc181d7))

## [6.13.0-beta.2](https://github.com/Belphemur/SoundSwitch/compare/v6.13.0-beta.1...v6.13.0-beta.2) (2025-02-24)

### Languages

* **German:** Translated Settings using Weblate ([9482857](https://github.com/Belphemur/SoundSwitch/commit/948285741777e37f2c2b0a0a9fb746eb4a14e249))

### Bug Fixes

* **cli:** add missing mute command ([8f298c5](https://github.com/Belphemur/SoundSwitch/commit/8f298c5ae97a67b106b729e4d1fccd343975ccbc))
* **installer:** Make the uninstaller delete any trace of the program ([464f05c](https://github.com/Belphemur/SoundSwitch/commit/464f05c6b356832b939d5e270fb77656b09d3557))
* **pipe:** be sure we can have full communication on the pipe ([1362535](https://github.com/Belphemur/SoundSwitch/commit/136253565858fdac0c6e9ecd9095bc8566eefaff))

### Features

* **cli::installer:** be sure the cli is installed and signed by the installer ([de470f6](https://github.com/Belphemur/SoundSwitch/commit/de470f63aed2c3d4c77e57f750a12f2b79f1dcc8))
* **cli:** add microphone muting to the CLI ([6613d2a](https://github.com/Belphemur/SoundSwitch/commit/6613d2ab918b277af5888d16ba85492767ecb0c7))
* **cli:** Add profile details like what device is setup in it ([0a3544d](https://github.com/Belphemur/SoundSwitch/commit/0a3544d1208908628c755b4b85f7891b5322fa92))
* **cli:** First version of the CLI for SoundSwitch ([add5240](https://github.com/Belphemur/SoundSwitch/commit/add524091f7b86407cf72c6e40bde312085fd540)), closes [#1664](https://github.com/Belphemur/SoundSwitch/issues/1664)
* **pipe:** implement MessagePack serialization for inter-process communication. Prepare the ground for a CLI. ([5b0f473](https://github.com/Belphemur/SoundSwitch/commit/5b0f473df5d064931e0372e4f0e8b7bf05257332))

## [6.13.0-beta.1](https://github.com/Belphemur/SoundSwitch/compare/v6.12.0...v6.13.0-beta.1) (2025-02-10)

### Enhancements

* **installer:** make installer compatible with ARM64 using compatibility layer ([fc31118](https://github.com/Belphemur/SoundSwitch/commit/fc311187ae154453fa7714e80d110ad8643db894))
* **ipc:** Be sure two instance of SoundSwitch can communicate. ([118681a](https://github.com/Belphemur/SoundSwitch/commit/118681a9a015e99f1580c6691641c8cecac0c2e5))
* **Reset Audio:** Bring back shortcut to reset per app audio ([b87f96c](https://github.com/Belphemur/SoundSwitch/commit/b87f96c40cdb5b3b9efb75adf1fb146ff162790e)), closes [#1630](https://github.com/Belphemur/SoundSwitch/issues/1630) [#1539](https://github.com/Belphemur/SoundSwitch/issues/1539) [#860](https://github.com/Belphemur/SoundSwitch/issues/860) [#1107](https://github.com/Belphemur/SoundSwitch/issues/1107) [#1515](https://github.com/Belphemur/SoundSwitch/issues/1515)

### Languages

* **Thai:** Translated Settings using Weblate ([75a58b3](https://github.com/Belphemur/SoundSwitch/commit/75a58b3eccab2b533b278281c0f3b134a7e59831))

### Bug Fixes

* **communication:** Possible crash on closing the software ([5b8fd5c](https://github.com/Belphemur/SoundSwitch/commit/5b8fd5c49c98d8766e2c8ec6134ae24a16cc0181))
* **release:** fix building right version of application ([ba16256](https://github.com/Belphemur/SoundSwitch/commit/ba16256de99e4119b947a33a4175a6d13709c86f)), closes [#1669](https://github.com/Belphemur/SoundSwitch/issues/1669)

### Features

* **Settings:** reopen settings when opening another SoundSwitch. ([22920cd](https://github.com/Belphemur/SoundSwitch/commit/22920cd2f9c2c279f612127245db433832963590)), closes [#850](https://github.com/Belphemur/SoundSwitch/issues/850)

## [6.12.0](https://github.com/Belphemur/SoundSwitch/compare/v6.11.0...v6.12.0) (2025-02-05)

### Languages

* **Arabic:** Translated Settings using Weblate ([f1fc3ff](https://github.com/Belphemur/SoundSwitch/commit/f1fc3ff797feff2a005966e621614e8a2f056b83))
* **Croatian:** Translated Settings using Weblate ([bbf054b](https://github.com/Belphemur/SoundSwitch/commit/bbf054b6f068b5ad557ef081f7987e5f4daef929))
* **Croatian:** Translated Settings using Weblate ([29b1c3f](https://github.com/Belphemur/SoundSwitch/commit/29b1c3f314d353a82269970fe8744da2a78ccf3e))
* **Danish:** Added Tray Icon translation using Weblate ([e86d491](https://github.com/Belphemur/SoundSwitch/commit/e86d491500f5fdb8f776212fc479c2b29b16b920))
* **Danish:** Added Update Download translation using Weblate ([fa179c9](https://github.com/Belphemur/SoundSwitch/commit/fa179c9a473432f7d12c61ba77943dceef2e7139))
* **Danish:** Translated Settings using Weblate ([97b7cbd](https://github.com/Belphemur/SoundSwitch/commit/97b7cbd841a12c9d565b6e7f2b8ca59c3e9fb4c6))
* **Danish:** Translated Settings using Weblate ([f9de1ab](https://github.com/Belphemur/SoundSwitch/commit/f9de1ab58680b299644b0befe570449b8f2e2121))
* **Danish:** Translated Settings using Weblate ([8e79d8a](https://github.com/Belphemur/SoundSwitch/commit/8e79d8af036d5ea822bfb313f3a2f0dae33cad8a))
* **Dutch:** Translated Settings using Weblate ([8e1e1be](https://github.com/Belphemur/SoundSwitch/commit/8e1e1be32e6ef848de19069aad1f99e3f2e350e3))
* **Finnish:** Translated About using Weblate ([15b0003](https://github.com/Belphemur/SoundSwitch/commit/15b0003e11f5b20cfcafed12f2068845a87e85b9))
* **Greek:** Translated About using Weblate ([9ba7cfb](https://github.com/Belphemur/SoundSwitch/commit/9ba7cfb63b3079ab55583e95b0c49791437df223))
* **Hebrew:** Translated Settings using Weblate ([c12486c](https://github.com/Belphemur/SoundSwitch/commit/c12486cec92d75e41d17f89e6c801a21329586d3))
* **Korean:** Translated Settings using Weblate ([dea1edb](https://github.com/Belphemur/SoundSwitch/commit/dea1edbdb5c00f50ae3479321f3b56f28d2060d1))
* **Korean:** Translated Settings using Weblate ([547b10f](https://github.com/Belphemur/SoundSwitch/commit/547b10f136ff05a37d4c5b02cf2fe7cee7bb2b51))
* **Portuguese (Brazil):** Translated Settings using Weblate ([f1953c4](https://github.com/Belphemur/SoundSwitch/commit/f1953c42df2bc41b82f1ca176d032eafb1f8e6ba))
* **Portuguese (Brazil):** Translated Settings using Weblate ([f37ec33](https://github.com/Belphemur/SoundSwitch/commit/f37ec3388491c5f9364d685b7a9948a8f59f79eb))
* **Russian:** Translated Settings using Weblate ([89ef328](https://github.com/Belphemur/SoundSwitch/commit/89ef3284fd69f1c535654a18986a220dddc8e629))
* **Russian:** Translated Settings using Weblate ([006d8ec](https://github.com/Belphemur/SoundSwitch/commit/006d8ec6406888dffd41275e82c1067a5a4ffa81))
* **Spanish:** Translated Settings using Weblate ([409ecd5](https://github.com/Belphemur/SoundSwitch/commit/409ecd5caafa72f2c09594e0b280544f04ce7b3b))
* **Swedish:** Translated Settings using Weblate ([1595260](https://github.com/Belphemur/SoundSwitch/commit/1595260a9a68c600468bdae60cda0a8375875337))
* **Swedish:** Translated Settings using Weblate ([101c56d](https://github.com/Belphemur/SoundSwitch/commit/101c56d9ac4a304ef8aec87835989a8869dce067))
* **Ukrainian:** Translated Settings using Weblate ([29a4254](https://github.com/Belphemur/SoundSwitch/commit/29a4254e5813919182f0d495a409feeef1c0a307))
* **Ukrainian:** Translated Settings using Weblate ([5ac9654](https://github.com/Belphemur/SoundSwitch/commit/5ac96547486b4cab15782f048be600560fc06220))
* **Ukrainian:** Translated Settings using Weblate ([8caacba](https://github.com/Belphemur/SoundSwitch/commit/8caacbade53f3477126fc2d627fdfe484cab2cd8))
* **Ukrainian:** Translated Settings using Weblate ([bd3f44e](https://github.com/Belphemur/SoundSwitch/commit/bd3f44ea2f4ad3b94f493adc79006c71cfb79ed4))
* **Ukrainian:** Translated Settings using Weblate ([21515db](https://github.com/Belphemur/SoundSwitch/commit/21515dbc932c124c100197f9e20841c8375d7ad3))

### Bug Fixes

* **.NET:** Fix components for .NET 9.0 ([ec45a8e](https://github.com/Belphemur/SoundSwitch/commit/ec45a8ebe6bf928eb86caacce3b17b0697fc0f13))
* file format ([6ea931e](https://github.com/Belphemur/SoundSwitch/commit/6ea931e223a26ec4d1286ea79430936fde274797))
* formatting of file ([f633666](https://github.com/Belphemur/SoundSwitch/commit/f6336663ecbb2c10a9163792a6c0e7b0ebf9e0cf))
* **Fornite:** Fix SoundSwitch compatibility with Fornite ([ea25a25](https://github.com/Belphemur/SoundSwitch/commit/ea25a2545570357d2be34802b9748fada9af33d2)), closes [#1588](https://github.com/Belphemur/SoundSwitch/issues/1588) [#1553](https://github.com/Belphemur/SoundSwitch/issues/1553)
* **fortnite:** possible fix with Fortnite freeze SoundSwitch ([04ad1ee](https://github.com/Belphemur/SoundSwitch/commit/04ad1eecdc2f3f648a4a774c2de327a1bcb2c643))
* **profile:steam big picture:** Fix detecting big picture in different languages ([78fc229](https://github.com/Belphemur/SoundSwitch/commit/78fc229d153da1af20efd0c2f2b33fdeafd46858)), closes [#1403](https://github.com/Belphemur/SoundSwitch/issues/1403)
* **profile:steam big picture:** Fix detecting big picture in most language ([12bd798](https://github.com/Belphemur/SoundSwitch/commit/12bd798178beaf42b8ac533a0a97bb676bb92c72)), closes [#1403](https://github.com/Belphemur/SoundSwitch/issues/1403)

### Features

* **.NET:** move to .NET 9.0 ([53878ab](https://github.com/Belphemur/SoundSwitch/commit/53878ab8576b36daf6d4ccbc4eb830b364fc9c77))

## [6.11.0](https://github.com/Belphemur/SoundSwitch/compare/v6.10.3...v6.11.0) (2024-06-04)

### Enhancements

* **banner:** Added Center Position ([60e5954](https://github.com/Belphemur/SoundSwitch/commit/60e59543244e3ae0021358270ec70322273c7f28))
* **center:** Middle of the screen/center position for accessibility ([9053231](https://github.com/Belphemur/SoundSwitch/commit/9053231f4c3dfc3f21b80f01767a4090da2a4c5b)), closes [#1466](https://github.com/Belphemur/SoundSwitch/issues/1466)
* **language:** Add Swedish translations ([27ef51f](https://github.com/Belphemur/SoundSwitch/commit/27ef51fe7911f047d2a79e9171a35bdf035fe593)), closes [#1431](https://github.com/Belphemur/SoundSwitch/issues/1431)
* **profile:** improve description of profile and let the text auto wrap ([551c41b](https://github.com/Belphemur/SoundSwitch/commit/551c41b97c7cdd08a8ed2e4e94efbd7cc5791d5c)), closes [#1433](https://github.com/Belphemur/SoundSwitch/issues/1433)
* **settings): Troubleshooting; refactor(trayIcon:** Updated Tray Menu ([f132a89](https://github.com/Belphemur/SoundSwitch/commit/f132a894483f06056cfdf85690aabfdc597b2ca3))
* **updater:** auto retry when can't download the file ([51bac9c](https://github.com/Belphemur/SoundSwitch/commit/51bac9c4d027d163e7516e3537eff812bcde1741))

### Languages

* **Chinese (Simplified):** Translated Settings using Weblate ([c08e449](https://github.com/Belphemur/SoundSwitch/commit/c08e4499a498c8cd31a79fd232c1168f7b5d1d55))
* **Croatian:** Translated About using Weblate ([b16e510](https://github.com/Belphemur/SoundSwitch/commit/b16e510afd7003698505ced4c7b30e5a0229fa83))
* **Croatian:** Translated Settings using Weblate ([4384e8c](https://github.com/Belphemur/SoundSwitch/commit/4384e8cbe03bac434bfcd5b97e78c19d08100b50))
* **Danish:** Translated Settings using Weblate ([6d5d619](https://github.com/Belphemur/SoundSwitch/commit/6d5d619e44af73bf63d8859524e279d4e1651be5))
* **Dutch:** Translated Settings using Weblate ([7c48d01](https://github.com/Belphemur/SoundSwitch/commit/7c48d01e0e15d9a4d9b345be304bb797231b8524))
* **Dutch:** Translated Settings using Weblate ([3103328](https://github.com/Belphemur/SoundSwitch/commit/31033283a4b13a630f2c5311945b40937ccb22f3))
* **Dutch:** Translated Update Download using Weblate ([681c508](https://github.com/Belphemur/SoundSwitch/commit/681c5083e763a0f09555c4b5daf72a21b086f91a))
* **English:** Translated Settings using Weblate ([c661601](https://github.com/Belphemur/SoundSwitch/commit/c66160145d273125c96b1cb0424198bdbe7a8315))
* **English:** Translated Settings using Weblate ([f30acf6](https://github.com/Belphemur/SoundSwitch/commit/f30acf6a133ff674149a0991e0fc7d8607e830f8))
* **French:** Translated Settings using Weblate ([765c837](https://github.com/Belphemur/SoundSwitch/commit/765c837d84b6309530f6541984480b8f9e26f0ae))
* **Hebrew:** Translated Settings using Weblate ([8ada5b1](https://github.com/Belphemur/SoundSwitch/commit/8ada5b1f6bf62bd43eac9fb1a20cee805f313d93))
* **Hebrew:** Translated Settings using Weblate ([36ba8bc](https://github.com/Belphemur/SoundSwitch/commit/36ba8bc02c7318f62fddffaa1ee098a0bc96fffe))
* **Hebrew:** Translated Settings using Weblate ([8e2ad75](https://github.com/Belphemur/SoundSwitch/commit/8e2ad75251600e9f4b5adb83d15ee70fabefa421))
* **Italian:** Translated Settings using Weblate ([5d306f2](https://github.com/Belphemur/SoundSwitch/commit/5d306f2711ce667322de2571fa8a7a7f52db9108))
* **Italian:** Translated Settings using Weblate ([6e9d2b5](https://github.com/Belphemur/SoundSwitch/commit/6e9d2b5fdc9421ba0483fbc8a296853fbb7280b8))
* **Japanese:** Translated About using Weblate ([5cedb13](https://github.com/Belphemur/SoundSwitch/commit/5cedb13c687a976c676efb6f565d66fe7e1737a3))
* **Japanese:** Translated About using Weblate ([8abbb54](https://github.com/Belphemur/SoundSwitch/commit/8abbb54ff10dbb069feee9dd79930b775c7e81e2))
* **Japanese:** Translated Settings using Weblate ([4ed10a0](https://github.com/Belphemur/SoundSwitch/commit/4ed10a0bf7d75582433b0360d9a7e100e3b94c78))
* **Japanese:** Translated Settings using Weblate ([2b89880](https://github.com/Belphemur/SoundSwitch/commit/2b89880ea87a7696b2fd26c5fb1ef9267afae73f))
* **Japanese:** Translated Settings using Weblate ([94629ce](https://github.com/Belphemur/SoundSwitch/commit/94629ceb1334521dd7873c6e2e31c789b33ef7aa))
* **Japanese:** Translated Settings using Weblate ([27e14d9](https://github.com/Belphemur/SoundSwitch/commit/27e14d90a74da9403668d82da9c6ee631aaba9f7))
* **Japanese:** Translated Settings using Weblate ([d82cb2b](https://github.com/Belphemur/SoundSwitch/commit/d82cb2be155735f8d5a0244b0feb9372bd3cb606))
* **Japanese:** Translated Settings using Weblate ([8aa4f82](https://github.com/Belphemur/SoundSwitch/commit/8aa4f82d41ae62f673099b99557cf410aeb7bc29))
* **Japanese:** Translated Settings using Weblate ([00ed06c](https://github.com/Belphemur/SoundSwitch/commit/00ed06c169c796a2b3eb2c63ede84fc7d347c85a))
* **Japanese:** Translated Settings using Weblate ([fde374e](https://github.com/Belphemur/SoundSwitch/commit/fde374e2c689a5c9e3ec83dac8fa7d710f3ca945))
* **Japanese:** Translated Tray Icon using Weblate ([8c97167](https://github.com/Belphemur/SoundSwitch/commit/8c97167020d4cd698ef15d7af555a0632c8945d0))
* **Japanese:** Translated Tray Icon using Weblate ([5f0de1b](https://github.com/Belphemur/SoundSwitch/commit/5f0de1bc1a06d5ee41ae348cf88a6b80eb992853))
* **Japanese:** Translated Update Download using Weblate ([872a51f](https://github.com/Belphemur/SoundSwitch/commit/872a51f290faa7d3b847dfcb83b40315f1e70a4c))
* **Japanese:** Translated Update Download using Weblate ([525ae92](https://github.com/Belphemur/SoundSwitch/commit/525ae92dfb2d8d996346ab5e56de115090604006))
* **Spanish:** Translated Settings using Weblate ([dd90865](https://github.com/Belphemur/SoundSwitch/commit/dd9086585df1a89233adc0f58de5eb4f3fbbfc4a))
* **Spanish:** Translated Settings using Weblate ([dc8fe47](https://github.com/Belphemur/SoundSwitch/commit/dc8fe472ac9dce2ab3f574419e67d8f50cd71a59))
* **Spanish:** Translated Settings using Weblate ([1c343e6](https://github.com/Belphemur/SoundSwitch/commit/1c343e65f2257f701208e32f26ea8400111afb8e))
* **Spanish:** Translated Settings using Weblate ([57ff02b](https://github.com/Belphemur/SoundSwitch/commit/57ff02bac55fc7238d7c2fd5786cc191b1c3b1d3))
* **Swedish:** Translated Settings using Weblate ([33dbc50](https://github.com/Belphemur/SoundSwitch/commit/33dbc502ad3ed5b3bd55acfa2f9f807cc5402566))

### Bug Fixes

* **banner/quickmenu:** Don't show notification or quick menu in the ALT+TAB menu ([5312f64](https://github.com/Belphemur/SoundSwitch/commit/5312f64a79f9657ad0146c2a0346833bdd2888bc)), closes [#1475](https://github.com/Belphemur/SoundSwitch/issues/1475)
* **banner:** banner crashing randomnly ([8ffa6d9](https://github.com/Belphemur/SoundSwitch/commit/8ffa6d9dc5771551a5ab16d3c4d8f1f587324aee))
* **ci:** downgrade conventianl commits ([c54c3e0](https://github.com/Belphemur/SoundSwitch/commit/c54c3e0c3732fc4096e28a627a65e8f5d78dc424))
* **ci:** fix dev version calculation ([81253d4](https://github.com/Belphemur/SoundSwitch/commit/81253d4652e85b597bdea14caf392c6d13085b8f))
* dev version calculation ([8fecb98](https://github.com/Belphemur/SoundSwitch/commit/8fecb98aca7ba7a62bad9cbad138cc4c5f295949))
* **localization:** Updated Settings and TrayIcon Entries ([145f98d](https://github.com/Belphemur/SoundSwitch/commit/145f98d4542f0ebaaa285b153cc507d7fb61859c))
* **profile:** fix crashing when switching profile ([0bf93e6](https://github.com/Belphemur/SoundSwitch/commit/0bf93e6879b03e6a5369646e87a86b652f96a60c)), closes [#1481](https://github.com/Belphemur/SoundSwitch/issues/1481)
* **profiles:** Readjusted items in Profiles tab ([75c4ea0](https://github.com/Belphemur/SoundSwitch/commit/75c4ea092a074e15238eed23a82be53bae4664cd))
* **profile:** used icon for profile ([8a8237b](https://github.com/Belphemur/SoundSwitch/commit/8a8237b4c53bd375304edeaac41cebf45e09a83a))
* **quickmenu:** Fix crash with QuickMenu having disposed icon ([6a79ec9](https://github.com/Belphemur/SoundSwitch/commit/6a79ec9f03c4f3397c8bb32bbb655bc85865cec0))
* **quickmenu:** possible crash when quick menu triggered while disappearing ([bd51528](https://github.com/Belphemur/SoundSwitch/commit/bd51528ea4e23f22c5f96b59183d2afe323e15c3))
* **Settings:** Fix issue where the settings close button could overlap with text. ([f5ceed7](https://github.com/Belphemur/SoundSwitch/commit/f5ceed7cceecc5ec0fc29222496e3c56a0c41ed3)), closes [#1448](https://github.com/Belphemur/SoundSwitch/issues/1448)

### Features

* **banner:on-screen-time:** Settings to change how long the banner stays on the screen ([41644f5](https://github.com/Belphemur/SoundSwitch/commit/41644f520ebd63fff292e9661db597fdba806c82)), closes [#1467](https://github.com/Belphemur/SoundSwitch/issues/1467)
* **notification:** Make the number of banner notification on the screen configurable ([cc15647](https://github.com/Belphemur/SoundSwitch/commit/cc15647b42f39265edc0f6186b0bb64ee93578d6))

## [6.11.0-beta.2](https://github.com/Belphemur/SoundSwitch/compare/v6.11.0-beta.1...v6.11.0-beta.2) (2024-05-22)


### Languages

* **Danish:** Translated Settings using Weblate ([6d5d619](https://github.com/Belphemur/SoundSwitch/commit/6d5d619e44af73bf63d8859524e279d4e1651be5))
* **Japanese:** Translated Settings using Weblate ([2b89880](https://github.com/Belphemur/SoundSwitch/commit/2b89880ea87a7696b2fd26c5fb1ef9267afae73f))
* **Spanish:** Translated Settings using Weblate ([dd90865](https://github.com/Belphemur/SoundSwitch/commit/dd9086585df1a89233adc0f58de5eb4f3fbbfc4a))


### Bug Fixes

* **ci:** downgrade conventianl commits ([c54c3e0](https://github.com/Belphemur/SoundSwitch/commit/c54c3e0c3732fc4096e28a627a65e8f5d78dc424))
* **ci:** fix dev version calculation ([81253d4](https://github.com/Belphemur/SoundSwitch/commit/81253d4652e85b597bdea14caf392c6d13085b8f))
* dev version calculation ([8fecb98](https://github.com/Belphemur/SoundSwitch/commit/8fecb98aca7ba7a62bad9cbad138cc4c5f295949))
* **profile:** fix crashing when switching profile ([0bf93e6](https://github.com/Belphemur/SoundSwitch/commit/0bf93e6879b03e6a5369646e87a86b652f96a60c)), closes [#1481](https://github.com/Belphemur/SoundSwitch/issues/1481)

## [6.11.0-beta.1](https://github.com/Belphemur/SoundSwitch/compare/v6.10.3...v6.11.0-beta.1) (2024-05-07)


### Enhancements

* **banner:** Added Center Position ([60e5954](https://github.com/Belphemur/SoundSwitch/commit/60e59543244e3ae0021358270ec70322273c7f28))
* **center:** Middle of the screen/center position for accessibility ([9053231](https://github.com/Belphemur/SoundSwitch/commit/9053231f4c3dfc3f21b80f01767a4090da2a4c5b)), closes [#1466](https://github.com/Belphemur/SoundSwitch/issues/1466)
* **language:** Add Swedish translations ([27ef51f](https://github.com/Belphemur/SoundSwitch/commit/27ef51fe7911f047d2a79e9171a35bdf035fe593)), closes [#1431](https://github.com/Belphemur/SoundSwitch/issues/1431)
* **profile:** improve description of profile and let the text auto wrap ([551c41b](https://github.com/Belphemur/SoundSwitch/commit/551c41b97c7cdd08a8ed2e4e94efbd7cc5791d5c)), closes [#1433](https://github.com/Belphemur/SoundSwitch/issues/1433)
* **settings): Troubleshooting; refactor(trayIcon:** Updated Tray Menu ([f132a89](https://github.com/Belphemur/SoundSwitch/commit/f132a894483f06056cfdf85690aabfdc597b2ca3))
* **updater:** auto retry when can't download the file ([51bac9c](https://github.com/Belphemur/SoundSwitch/commit/51bac9c4d027d163e7516e3537eff812bcde1741))


### Languages

* **Croatian:** Translated About using Weblate ([b16e510](https://github.com/Belphemur/SoundSwitch/commit/b16e510afd7003698505ced4c7b30e5a0229fa83))
* **Croatian:** Translated Settings using Weblate ([4384e8c](https://github.com/Belphemur/SoundSwitch/commit/4384e8cbe03bac434bfcd5b97e78c19d08100b50))
* **Dutch:** Translated Settings using Weblate ([7c48d01](https://github.com/Belphemur/SoundSwitch/commit/7c48d01e0e15d9a4d9b345be304bb797231b8524))
* **Dutch:** Translated Settings using Weblate ([3103328](https://github.com/Belphemur/SoundSwitch/commit/31033283a4b13a630f2c5311945b40937ccb22f3))
* **Dutch:** Translated Update Download using Weblate ([681c508](https://github.com/Belphemur/SoundSwitch/commit/681c5083e763a0f09555c4b5daf72a21b086f91a))
* **English:** Translated Settings using Weblate ([c661601](https://github.com/Belphemur/SoundSwitch/commit/c66160145d273125c96b1cb0424198bdbe7a8315))
* **English:** Translated Settings using Weblate ([f30acf6](https://github.com/Belphemur/SoundSwitch/commit/f30acf6a133ff674149a0991e0fc7d8607e830f8))
* **French:** Translated Settings using Weblate ([765c837](https://github.com/Belphemur/SoundSwitch/commit/765c837d84b6309530f6541984480b8f9e26f0ae))
* **Hebrew:** Translated Settings using Weblate ([8ada5b1](https://github.com/Belphemur/SoundSwitch/commit/8ada5b1f6bf62bd43eac9fb1a20cee805f313d93))
* **Hebrew:** Translated Settings using Weblate ([36ba8bc](https://github.com/Belphemur/SoundSwitch/commit/36ba8bc02c7318f62fddffaa1ee098a0bc96fffe))
* **Hebrew:** Translated Settings using Weblate ([8e2ad75](https://github.com/Belphemur/SoundSwitch/commit/8e2ad75251600e9f4b5adb83d15ee70fabefa421))
* **Italian:** Translated Settings using Weblate ([5d306f2](https://github.com/Belphemur/SoundSwitch/commit/5d306f2711ce667322de2571fa8a7a7f52db9108))
* **Italian:** Translated Settings using Weblate ([6e9d2b5](https://github.com/Belphemur/SoundSwitch/commit/6e9d2b5fdc9421ba0483fbc8a296853fbb7280b8))
* **Japanese:** Translated About using Weblate ([5cedb13](https://github.com/Belphemur/SoundSwitch/commit/5cedb13c687a976c676efb6f565d66fe7e1737a3))
* **Japanese:** Translated About using Weblate ([8abbb54](https://github.com/Belphemur/SoundSwitch/commit/8abbb54ff10dbb069feee9dd79930b775c7e81e2))
* **Japanese:** Translated Settings using Weblate ([94629ce](https://github.com/Belphemur/SoundSwitch/commit/94629ceb1334521dd7873c6e2e31c789b33ef7aa))
* **Japanese:** Translated Settings using Weblate ([27e14d9](https://github.com/Belphemur/SoundSwitch/commit/27e14d90a74da9403668d82da9c6ee631aaba9f7))
* **Japanese:** Translated Settings using Weblate ([d82cb2b](https://github.com/Belphemur/SoundSwitch/commit/d82cb2be155735f8d5a0244b0feb9372bd3cb606))
* **Japanese:** Translated Settings using Weblate ([8aa4f82](https://github.com/Belphemur/SoundSwitch/commit/8aa4f82d41ae62f673099b99557cf410aeb7bc29))
* **Japanese:** Translated Settings using Weblate ([00ed06c](https://github.com/Belphemur/SoundSwitch/commit/00ed06c169c796a2b3eb2c63ede84fc7d347c85a))
* **Japanese:** Translated Settings using Weblate ([fde374e](https://github.com/Belphemur/SoundSwitch/commit/fde374e2c689a5c9e3ec83dac8fa7d710f3ca945))
* **Japanese:** Translated Tray Icon using Weblate ([8c97167](https://github.com/Belphemur/SoundSwitch/commit/8c97167020d4cd698ef15d7af555a0632c8945d0))
* **Japanese:** Translated Tray Icon using Weblate ([5f0de1b](https://github.com/Belphemur/SoundSwitch/commit/5f0de1bc1a06d5ee41ae348cf88a6b80eb992853))
* **Japanese:** Translated Update Download using Weblate ([872a51f](https://github.com/Belphemur/SoundSwitch/commit/872a51f290faa7d3b847dfcb83b40315f1e70a4c))
* **Japanese:** Translated Update Download using Weblate ([525ae92](https://github.com/Belphemur/SoundSwitch/commit/525ae92dfb2d8d996346ab5e56de115090604006))
* **Spanish:** Translated Settings using Weblate ([dc8fe47](https://github.com/Belphemur/SoundSwitch/commit/dc8fe472ac9dce2ab3f574419e67d8f50cd71a59))
* **Spanish:** Translated Settings using Weblate ([1c343e6](https://github.com/Belphemur/SoundSwitch/commit/1c343e65f2257f701208e32f26ea8400111afb8e))
* **Spanish:** Translated Settings using Weblate ([57ff02b](https://github.com/Belphemur/SoundSwitch/commit/57ff02bac55fc7238d7c2fd5786cc191b1c3b1d3))


### Bug Fixes

* **banner/quickmenu:** Don't show notification or quick menu in the ALT+TAB menu ([5312f64](https://github.com/Belphemur/SoundSwitch/commit/5312f64a79f9657ad0146c2a0346833bdd2888bc)), closes [#1475](https://github.com/Belphemur/SoundSwitch/issues/1475)
* **localization:** Updated Settings and TrayIcon Entries ([145f98d](https://github.com/Belphemur/SoundSwitch/commit/145f98d4542f0ebaaa285b153cc507d7fb61859c))
* **profiles:** Readjusted items in Profiles tab ([75c4ea0](https://github.com/Belphemur/SoundSwitch/commit/75c4ea092a074e15238eed23a82be53bae4664cd))
* **profile:** used icon for profile ([8a8237b](https://github.com/Belphemur/SoundSwitch/commit/8a8237b4c53bd375304edeaac41cebf45e09a83a))
* **quickmenu:** Fix crash with QuickMenu having disposed icon ([6a79ec9](https://github.com/Belphemur/SoundSwitch/commit/6a79ec9f03c4f3397c8bb32bbb655bc85865cec0))
* **quickmenu:** possible crash when quick menu triggered while disappearing ([bd51528](https://github.com/Belphemur/SoundSwitch/commit/bd51528ea4e23f22c5f96b59183d2afe323e15c3))
* **Settings:** Fix issue where the settings close button could overlap with text. ([f5ceed7](https://github.com/Belphemur/SoundSwitch/commit/f5ceed7cceecc5ec0fc29222496e3c56a0c41ed3)), closes [#1448](https://github.com/Belphemur/SoundSwitch/issues/1448)


### Features

* **banner:on-screen-time:** Settings to change how long the banner stays on the screen ([41644f5](https://github.com/Belphemur/SoundSwitch/commit/41644f520ebd63fff292e9661db597fdba806c82)), closes [#1467](https://github.com/Belphemur/SoundSwitch/issues/1467)
* **notification:** Make the number of banner notification on the screen configurable ([cc15647](https://github.com/Belphemur/SoundSwitch/commit/cc15647b42f39265edc0f6186b0bb64ee93578d6))

## [6.11.0-beta.1](https://github.com/Belphemur/SoundSwitch/compare/v6.10.3...v6.11.0-beta.1) (2024-04-26)


### Enhancements

* **language:** Add Swedish translations ([27ef51f](https://github.com/Belphemur/SoundSwitch/commit/27ef51fe7911f047d2a79e9171a35bdf035fe593)), closes [#1431](https://github.com/Belphemur/SoundSwitch/issues/1431)
* **profile:** improve description of profile and let the text auto wrap ([551c41b](https://github.com/Belphemur/SoundSwitch/commit/551c41b97c7cdd08a8ed2e4e94efbd7cc5791d5c)), closes [#1433](https://github.com/Belphemur/SoundSwitch/issues/1433)
* **settings): Troubleshooting; refactor(trayIcon:** Updated Tray Menu ([f132a89](https://github.com/Belphemur/SoundSwitch/commit/f132a894483f06056cfdf85690aabfdc597b2ca3))
* **updater:** auto retry when can't download the file ([51bac9c](https://github.com/Belphemur/SoundSwitch/commit/51bac9c4d027d163e7516e3537eff812bcde1741))


### Languages

* **Croatian:** Translated About using Weblate ([b16e510](https://github.com/Belphemur/SoundSwitch/commit/b16e510afd7003698505ced4c7b30e5a0229fa83))
* **Croatian:** Translated Settings using Weblate ([4384e8c](https://github.com/Belphemur/SoundSwitch/commit/4384e8cbe03bac434bfcd5b97e78c19d08100b50))
* **Dutch:** Translated Settings using Weblate ([7c48d01](https://github.com/Belphemur/SoundSwitch/commit/7c48d01e0e15d9a4d9b345be304bb797231b8524))
* **Dutch:** Translated Settings using Weblate ([3103328](https://github.com/Belphemur/SoundSwitch/commit/31033283a4b13a630f2c5311945b40937ccb22f3))
* **Dutch:** Translated Update Download using Weblate ([681c508](https://github.com/Belphemur/SoundSwitch/commit/681c5083e763a0f09555c4b5daf72a21b086f91a))
* **English:** Translated Settings using Weblate ([c661601](https://github.com/Belphemur/SoundSwitch/commit/c66160145d273125c96b1cb0424198bdbe7a8315))
* **English:** Translated Settings using Weblate ([f30acf6](https://github.com/Belphemur/SoundSwitch/commit/f30acf6a133ff674149a0991e0fc7d8607e830f8))
* **Hebrew:** Translated Settings using Weblate ([8ada5b1](https://github.com/Belphemur/SoundSwitch/commit/8ada5b1f6bf62bd43eac9fb1a20cee805f313d93))
* **Hebrew:** Translated Settings using Weblate ([36ba8bc](https://github.com/Belphemur/SoundSwitch/commit/36ba8bc02c7318f62fddffaa1ee098a0bc96fffe))
* **Hebrew:** Translated Settings using Weblate ([8e2ad75](https://github.com/Belphemur/SoundSwitch/commit/8e2ad75251600e9f4b5adb83d15ee70fabefa421))
* **Italian:** Translated Settings using Weblate ([6e9d2b5](https://github.com/Belphemur/SoundSwitch/commit/6e9d2b5fdc9421ba0483fbc8a296853fbb7280b8))
* **Japanese:** Translated About using Weblate ([5cedb13](https://github.com/Belphemur/SoundSwitch/commit/5cedb13c687a976c676efb6f565d66fe7e1737a3))
* **Japanese:** Translated About using Weblate ([8abbb54](https://github.com/Belphemur/SoundSwitch/commit/8abbb54ff10dbb069feee9dd79930b775c7e81e2))
* **Japanese:** Translated Settings using Weblate ([27e14d9](https://github.com/Belphemur/SoundSwitch/commit/27e14d90a74da9403668d82da9c6ee631aaba9f7))
* **Japanese:** Translated Settings using Weblate ([d82cb2b](https://github.com/Belphemur/SoundSwitch/commit/d82cb2be155735f8d5a0244b0feb9372bd3cb606))
* **Japanese:** Translated Settings using Weblate ([8aa4f82](https://github.com/Belphemur/SoundSwitch/commit/8aa4f82d41ae62f673099b99557cf410aeb7bc29))
* **Japanese:** Translated Settings using Weblate ([00ed06c](https://github.com/Belphemur/SoundSwitch/commit/00ed06c169c796a2b3eb2c63ede84fc7d347c85a))
* **Japanese:** Translated Settings using Weblate ([fde374e](https://github.com/Belphemur/SoundSwitch/commit/fde374e2c689a5c9e3ec83dac8fa7d710f3ca945))
* **Japanese:** Translated Tray Icon using Weblate ([8c97167](https://github.com/Belphemur/SoundSwitch/commit/8c97167020d4cd698ef15d7af555a0632c8945d0))
* **Japanese:** Translated Tray Icon using Weblate ([5f0de1b](https://github.com/Belphemur/SoundSwitch/commit/5f0de1bc1a06d5ee41ae348cf88a6b80eb992853))
* **Japanese:** Translated Update Download using Weblate ([872a51f](https://github.com/Belphemur/SoundSwitch/commit/872a51f290faa7d3b847dfcb83b40315f1e70a4c))
* **Japanese:** Translated Update Download using Weblate ([525ae92](https://github.com/Belphemur/SoundSwitch/commit/525ae92dfb2d8d996346ab5e56de115090604006))
* **Spanish:** Translated Settings using Weblate ([1c343e6](https://github.com/Belphemur/SoundSwitch/commit/1c343e65f2257f701208e32f26ea8400111afb8e))
* **Spanish:** Translated Settings using Weblate ([57ff02b](https://github.com/Belphemur/SoundSwitch/commit/57ff02bac55fc7238d7c2fd5786cc191b1c3b1d3))


### Bug Fixes

* **localization:** Updated Settings and TrayIcon Entries ([145f98d](https://github.com/Belphemur/SoundSwitch/commit/145f98d4542f0ebaaa285b153cc507d7fb61859c))
* **profiles:** Readjusted items in Profiles tab ([75c4ea0](https://github.com/Belphemur/SoundSwitch/commit/75c4ea092a074e15238eed23a82be53bae4664cd))
* **profile:** used icon for profile ([8a8237b](https://github.com/Belphemur/SoundSwitch/commit/8a8237b4c53bd375304edeaac41cebf45e09a83a))
* **quickmenu:** Fix crash with QuickMenu having disposed icon ([6a79ec9](https://github.com/Belphemur/SoundSwitch/commit/6a79ec9f03c4f3397c8bb32bbb655bc85865cec0))
* **quickmenu:** possible crash when quick menu triggered while disappearing ([bd51528](https://github.com/Belphemur/SoundSwitch/commit/bd51528ea4e23f22c5f96b59183d2afe323e15c3))
* **Settings:** Fix issue where the settings close button could overlap with text. ([f5ceed7](https://github.com/Belphemur/SoundSwitch/commit/f5ceed7cceecc5ec0fc29222496e3c56a0c41ed3)), closes [#1448](https://github.com/Belphemur/SoundSwitch/issues/1448)


### Features

* **notification:** Make the number of banner notification on the screen configurable ([cc15647](https://github.com/Belphemur/SoundSwitch/commit/cc15647b42f39265edc0f6186b0bb64ee93578d6))

## [6.10.3](https://github.com/Belphemur/SoundSwitch/compare/v6.10.2...v6.10.3) (2024-04-07)


### Enhancements

* **icon:** improve the logic that cache device icons ([886bd81](https://github.com/Belphemur/SoundSwitch/commit/886bd81135f1dbb3b9f2557dc6b695e1fc21b51d))


### Bug Fixes

* **device:icon:** Force max size of 32px instead of what's available as large ([c389f27](https://github.com/Belphemur/SoundSwitch/commit/c389f2769a150f59893e1537e5728b1902ff485f))
* **settings:** fix issue with device list (settings) crashing when too many devices ([180ca33](https://github.com/Belphemur/SoundSwitch/commit/180ca33a43f9572588703cb8ad37330d563e2e20))

## [6.10.2](https://github.com/Belphemur/SoundSwitch/compare/v6.10.1...v6.10.2) (2024-04-07)


### Languages

* **Japanese:** Translated Settings using Weblate ([3bd9ef2](https://github.com/Belphemur/SoundSwitch/commit/3bd9ef2ee179d6f84d367050508b711eb43001e9))
* **Japanese:** Translated Tray Icon using Weblate ([3e5e936](https://github.com/Belphemur/SoundSwitch/commit/3e5e9366922feefbc9e52cb57d1bd4f9d755947e))


### Bug Fixes

* **settings:** Hide Primary Screen CheckBox and Position ComboBox when Banner Position not selected ([c775eb5](https://github.com/Belphemur/SoundSwitch/commit/c775eb5d7205401d9980dc21118ee0f6991409fa))

## [6.10.1-beta.1](https://github.com/Belphemur/SoundSwitch/compare/v6.10.0...v6.10.1-beta.1) (2024-04-05)


### Enhancements

* **banner:** Always have the latest banner at the top of the stack ([95575f9](https://github.com/Belphemur/SoundSwitch/commit/95575f976306afe434911181b4faf144c73e83dc))


### Bug Fixes

* **auto-update:** fix bootloop of SoundSwitch when auto-updating with the new beta ([e19a21d](https://github.com/Belphemur/SoundSwitch/commit/e19a21dc08a35d1b58b1a6e6ef77bfc626d1c794)), closes [#1422](https://github.com/Belphemur/SoundSwitch/issues/1422)

## [6.10.0-beta.1](https://github.com/Belphemur/SoundSwitch/compare/v6.9.0...v6.10.0-beta.1) (2024-04-05)


### Languages

* **Polish:** Translated Settings using Weblate ([54aeeac](https://github.com/Belphemur/SoundSwitch/commit/54aeeacda18db243e98b7a60f5d72d23a783f835))
* **Portuguese (Brazil):** Translated Settings using Weblate ([b8e50fb](https://github.com/Belphemur/SoundSwitch/commit/b8e50fb679054336eee55548835c438f15ded5c1))


### Bug Fixes

* **auto-update:** parsing of version with beta ([8ef4e51](https://github.com/Belphemur/SoundSwitch/commit/8ef4e516172e93fdb6fa0d7f948bdbd28fd781aa))
* **icon:** fix not saving in cache the icon ([8623e72](https://github.com/Belphemur/SoundSwitch/commit/8623e72f32cbf971bc3441ebbf9b91869f65aecd))
* **updater:** fix possible issue with the updater crashing at SoundSwitch startup. ([4de67e6](https://github.com/Belphemur/SoundSwitch/commit/4de67e69b0152c59d32187444f82e47e6ce14d5f))
* **version:** fix the file version of the app ([8de2d5b](https://github.com/Belphemur/SoundSwitch/commit/8de2d5b02597156f0d8e3545d0ee68beeabbfff3))


### Features

* **banner:** Make banner stackable instead of replacing content ([b26eb7c](https://github.com/Belphemur/SoundSwitch/commit/b26eb7c7f103eb7b9d7cc3a7ed69e0415aec53d6))

## [6.10.0-beta.1](https://github.com/Belphemur/SoundSwitch/compare/v6.9.0...v6.10.0-beta.1) (2024-04-05)


### Languages

* **Portuguese (Brazil):** Translated Settings using Weblate ([b8e50fb](https://github.com/Belphemur/SoundSwitch/commit/b8e50fb679054336eee55548835c438f15ded5c1))


### Bug Fixes

* **auto-update:** parsing of version with beta ([8ef4e51](https://github.com/Belphemur/SoundSwitch/commit/8ef4e516172e93fdb6fa0d7f948bdbd28fd781aa))
* **updater:** fix possible issue with the updater crashing at SoundSwitch startup. ([4de67e6](https://github.com/Belphemur/SoundSwitch/commit/4de67e69b0152c59d32187444f82e47e6ce14d5f))
* **version:** fix the file version of the app ([8de2d5b](https://github.com/Belphemur/SoundSwitch/commit/8de2d5b02597156f0d8e3545d0ee68beeabbfff3))


### Features

* **banner:** Make banner stackable instead of replacing content ([b26eb7c](https://github.com/Belphemur/SoundSwitch/commit/b26eb7c7f103eb7b9d7cc3a7ed69e0415aec53d6))

## [6.9.0-beta.1](https://github.com/Belphemur/SoundSwitch/compare/v6.8.1...v6.9.0-beta.1) (2024-04-03)


### Enhancements

* **notification:** Added Top Center/Bottom Center to Banner Positions ([#1347](https://github.com/Belphemur/SoundSwitch/issues/1347)) ([1566fc9](https://github.com/Belphemur/SoundSwitch/commit/1566fc9d092d04c8e5a2705d262d456332a81e6b))
* **notification:** Merged Custom Sound Notification into Sound Notification ([#1352](https://github.com/Belphemur/SoundSwitch/issues/1352)) ([a163a09](https://github.com/Belphemur/SoundSwitch/commit/a163a09443cca37d9ffcd5265d6c187f3b2b7d08))
* **volume:** Keep volume includes both playback and recording devices ([03d1aa8](https://github.com/Belphemur/SoundSwitch/commit/03d1aa8a1d90ae300eb482aa3e2d824ef9d4c429))


### Languages

* **Croatian:** Translated Settings using Weblate ([2503908](https://github.com/Belphemur/SoundSwitch/commit/2503908844547eec4cd160e63abf5df48d9272c9))
* **Croatian:** Translated Settings using Weblate ([9dcc946](https://github.com/Belphemur/SoundSwitch/commit/9dcc946fcab5d09ed7a312e562ff0a05e3d8875b))
* **Croatian:** Translated Settings using Weblate ([3ed5521](https://github.com/Belphemur/SoundSwitch/commit/3ed552140044c3c5be2d6e7f0f45fc9ebfe69077))
* **French:** Translated Settings using Weblate ([3d48b72](https://github.com/Belphemur/SoundSwitch/commit/3d48b72fd06a7b3983f30ccbce8d9490dd5b1e7c))
* **German:** Translated Settings using Weblate ([9d414d2](https://github.com/Belphemur/SoundSwitch/commit/9d414d2ae2ed0b6dfd2244827563886330f5517d))
* **German:** Translated Settings using Weblate ([c3598bf](https://github.com/Belphemur/SoundSwitch/commit/c3598bf772d3dff1545d395701a7ed4646345202))
* **Hebrew:** Translated Settings using Weblate ([00a65a5](https://github.com/Belphemur/SoundSwitch/commit/00a65a52177c33790ef9197784a02d78efc97c89))
* **Hungarian:** Translated Settings using Weblate ([97faf79](https://github.com/Belphemur/SoundSwitch/commit/97faf7906b19eec249cc7e739bc0bf820eb3f5f3))
* **Hungarian:** Translated Tray Icon using Weblate ([a191a49](https://github.com/Belphemur/SoundSwitch/commit/a191a49654cda649c9bcdacca0678d15bb400622))
* **Hungarian:** Translated Update Download using Weblate ([e85e988](https://github.com/Belphemur/SoundSwitch/commit/e85e9882b2bb5cd1ea665410250222697c38627f))
* **Portuguese:** Translated Settings using Weblate ([cccdf71](https://github.com/Belphemur/SoundSwitch/commit/cccdf7178c3e21effeee59260fceefee43b2ebf5))
* **Portuguese:** Translated Settings using Weblate ([b53b37b](https://github.com/Belphemur/SoundSwitch/commit/b53b37bebc00f3018e628915c4d719fb1c64ed0c))
* **Russian:** Translated Settings using Weblate ([1c93c71](https://github.com/Belphemur/SoundSwitch/commit/1c93c71586eff07f11916f3740d555c7d8b277dc))
* **Slovenian:** Translated About using Weblate ([15e1747](https://github.com/Belphemur/SoundSwitch/commit/15e1747f953f58c040e47152003e79fd4fd494ce))
* **Slovenian:** Translated Settings using Weblate ([a0feb07](https://github.com/Belphemur/SoundSwitch/commit/a0feb0769d9269568a4b1fd130d4dac095a7e428))
* **Spanish:** Translated Settings using Weblate ([b16f033](https://github.com/Belphemur/SoundSwitch/commit/b16f033e1f8330c38d78e3b3a5bd12c2530b8e91))
* **Spanish:** Translated Settings using Weblate ([9dc33bb](https://github.com/Belphemur/SoundSwitch/commit/9dc33bb3d0b6def3588641add836fc805e393d78))
* **Spanish:** Translated Settings using Weblate ([7569270](https://github.com/Belphemur/SoundSwitch/commit/75692705dccb7e4d5a96d4b2c04e6336cfa006ee))
* **Swedish:** Translated Settings using Weblate ([1bb6efb](https://github.com/Belphemur/SoundSwitch/commit/1bb6efb270cffbc5c5f54fc21cf5383ebeb3c411))
* **Swedish:** Translated Settings using Weblate ([6b3a6e7](https://github.com/Belphemur/SoundSwitch/commit/6b3a6e7f19892cd917e34edcc33fb793a6f374f5))
* **Swedish:** Translated Settings using Weblate ([a65951a](https://github.com/Belphemur/SoundSwitch/commit/a65951a231b536cc923fd7a7cbce879548295769))
* **Ukrainian:** Translated Settings using Weblate ([c3ed595](https://github.com/Belphemur/SoundSwitch/commit/c3ed595c12ae0cfaacb1eda43efea5cf9a7d9131))
* **Ukrainian:** Translated Tray Icon using Weblate ([48b11b7](https://github.com/Belphemur/SoundSwitch/commit/48b11b746c0fb16f77689499befba4cac052c67a))


### Bug Fixes

* comparing devices event, needs to also compare the id ([4851b43](https://github.com/Belphemur/SoundSwitch/commit/4851b438c0da0aa9c0ac34ceae1ac803ad5ed4bd))
* getting all device that have name ([d695d2d](https://github.com/Belphemur/SoundSwitch/commit/d695d2d38acb86f626db78afce19a0be869f6834))
* possible issue where device not removed from the list when renamed ([8ed0f14](https://github.com/Belphemur/SoundSwitch/commit/8ed0f1409c1ee1a02de5ba5e75a97ee644062c7c))
* **volume:** Volume levels across dual-audio channels now kept for keep volume option ([#1371](https://github.com/Belphemur/SoundSwitch/issues/1371)) ([7121ecb](https://github.com/Belphemur/SoundSwitch/commit/7121ecb1910f3afab555799bf53a2fdd12b127af))


### Features

* **autoadd:** remove the auto add feature ([dd39379](https://github.com/Belphemur/SoundSwitch/commit/dd39379f83180c711d0229cb7ff790372104af1c))
* make audioswitcher able to provide list of devices ([0d31968](https://github.com/Belphemur/SoundSwitch/commit/0d319688cbf27dbd0af4c1266503c16855ecc451))
* **notification:** Banner positions ([#1346](https://github.com/Belphemur/SoundSwitch/issues/1346)) ([40892fe](https://github.com/Belphemur/SoundSwitch/commit/40892fed2b678f9df0aa0d3a218d6b1e19d76261)), closes [#242](https://github.com/Belphemur/SoundSwitch/issues/242) [#1176](https://github.com/Belphemur/SoundSwitch/issues/1176)

## [6.9.0-beta.1](https://github.com/Belphemur/SoundSwitch/compare/v6.8.1...v6.9.0-beta.1) (2024-04-03)


### Enhancements

* **notification:** Added Top Center/Bottom Center to Banner Positions ([#1347](https://github.com/Belphemur/SoundSwitch/issues/1347)) ([1566fc9](https://github.com/Belphemur/SoundSwitch/commit/1566fc9d092d04c8e5a2705d262d456332a81e6b))
* **notification:** Merged Custom Sound Notification into Sound Notification ([#1352](https://github.com/Belphemur/SoundSwitch/issues/1352)) ([a163a09](https://github.com/Belphemur/SoundSwitch/commit/a163a09443cca37d9ffcd5265d6c187f3b2b7d08))
* **volume:** Keep volume includes both playback and recording devices ([03d1aa8](https://github.com/Belphemur/SoundSwitch/commit/03d1aa8a1d90ae300eb482aa3e2d824ef9d4c429))


### Languages

* **Croatian:** Translated Settings using Weblate ([2503908](https://github.com/Belphemur/SoundSwitch/commit/2503908844547eec4cd160e63abf5df48d9272c9))
* **Croatian:** Translated Settings using Weblate ([9dcc946](https://github.com/Belphemur/SoundSwitch/commit/9dcc946fcab5d09ed7a312e562ff0a05e3d8875b))
* **Croatian:** Translated Settings using Weblate ([3ed5521](https://github.com/Belphemur/SoundSwitch/commit/3ed552140044c3c5be2d6e7f0f45fc9ebfe69077))
* **French:** Translated Settings using Weblate ([3d48b72](https://github.com/Belphemur/SoundSwitch/commit/3d48b72fd06a7b3983f30ccbce8d9490dd5b1e7c))
* **German:** Translated Settings using Weblate ([9d414d2](https://github.com/Belphemur/SoundSwitch/commit/9d414d2ae2ed0b6dfd2244827563886330f5517d))
* **German:** Translated Settings using Weblate ([c3598bf](https://github.com/Belphemur/SoundSwitch/commit/c3598bf772d3dff1545d395701a7ed4646345202))
* **Hebrew:** Translated Settings using Weblate ([00a65a5](https://github.com/Belphemur/SoundSwitch/commit/00a65a52177c33790ef9197784a02d78efc97c89))
* **Hungarian:** Translated Settings using Weblate ([97faf79](https://github.com/Belphemur/SoundSwitch/commit/97faf7906b19eec249cc7e739bc0bf820eb3f5f3))
* **Hungarian:** Translated Tray Icon using Weblate ([a191a49](https://github.com/Belphemur/SoundSwitch/commit/a191a49654cda649c9bcdacca0678d15bb400622))
* **Hungarian:** Translated Update Download using Weblate ([e85e988](https://github.com/Belphemur/SoundSwitch/commit/e85e9882b2bb5cd1ea665410250222697c38627f))
* **Portuguese:** Translated Settings using Weblate ([cccdf71](https://github.com/Belphemur/SoundSwitch/commit/cccdf7178c3e21effeee59260fceefee43b2ebf5))
* **Portuguese:** Translated Settings using Weblate ([b53b37b](https://github.com/Belphemur/SoundSwitch/commit/b53b37bebc00f3018e628915c4d719fb1c64ed0c))
* **Russian:** Translated Settings using Weblate ([1c93c71](https://github.com/Belphemur/SoundSwitch/commit/1c93c71586eff07f11916f3740d555c7d8b277dc))
* **Slovenian:** Translated About using Weblate ([15e1747](https://github.com/Belphemur/SoundSwitch/commit/15e1747f953f58c040e47152003e79fd4fd494ce))
* **Slovenian:** Translated Settings using Weblate ([a0feb07](https://github.com/Belphemur/SoundSwitch/commit/a0feb0769d9269568a4b1fd130d4dac095a7e428))
* **Spanish:** Translated Settings using Weblate ([b16f033](https://github.com/Belphemur/SoundSwitch/commit/b16f033e1f8330c38d78e3b3a5bd12c2530b8e91))
* **Spanish:** Translated Settings using Weblate ([9dc33bb](https://github.com/Belphemur/SoundSwitch/commit/9dc33bb3d0b6def3588641add836fc805e393d78))
* **Spanish:** Translated Settings using Weblate ([7569270](https://github.com/Belphemur/SoundSwitch/commit/75692705dccb7e4d5a96d4b2c04e6336cfa006ee))
* **Swedish:** Translated Settings using Weblate ([1bb6efb](https://github.com/Belphemur/SoundSwitch/commit/1bb6efb270cffbc5c5f54fc21cf5383ebeb3c411))
* **Swedish:** Translated Settings using Weblate ([6b3a6e7](https://github.com/Belphemur/SoundSwitch/commit/6b3a6e7f19892cd917e34edcc33fb793a6f374f5))
* **Swedish:** Translated Settings using Weblate ([a65951a](https://github.com/Belphemur/SoundSwitch/commit/a65951a231b536cc923fd7a7cbce879548295769))
* **Ukrainian:** Translated Settings using Weblate ([c3ed595](https://github.com/Belphemur/SoundSwitch/commit/c3ed595c12ae0cfaacb1eda43efea5cf9a7d9131))
* **Ukrainian:** Translated Tray Icon using Weblate ([48b11b7](https://github.com/Belphemur/SoundSwitch/commit/48b11b746c0fb16f77689499befba4cac052c67a))


### Bug Fixes

* comparing devices event, needs to also compare the id ([4851b43](https://github.com/Belphemur/SoundSwitch/commit/4851b438c0da0aa9c0ac34ceae1ac803ad5ed4bd))
* getting all device that have name ([d695d2d](https://github.com/Belphemur/SoundSwitch/commit/d695d2d38acb86f626db78afce19a0be869f6834))
* possible issue where device not removed from the list when renamed ([8ed0f14](https://github.com/Belphemur/SoundSwitch/commit/8ed0f1409c1ee1a02de5ba5e75a97ee644062c7c))
* **volume:** Volume levels across dual-audio channels now kept for keep volume option ([#1371](https://github.com/Belphemur/SoundSwitch/issues/1371)) ([7121ecb](https://github.com/Belphemur/SoundSwitch/commit/7121ecb1910f3afab555799bf53a2fdd12b127af))


### Features

* **autoadd:** remove the auto add feature ([dd39379](https://github.com/Belphemur/SoundSwitch/commit/dd39379f83180c711d0229cb7ff790372104af1c))
* make audioswitcher able to provide list of devices ([0d31968](https://github.com/Belphemur/SoundSwitch/commit/0d319688cbf27dbd0af4c1266503c16855ecc451))
* **notification:** Banner positions ([#1346](https://github.com/Belphemur/SoundSwitch/issues/1346)) ([40892fe](https://github.com/Belphemur/SoundSwitch/commit/40892fed2b678f9df0aa0d3a218d6b1e19d76261)), closes [#242](https://github.com/Belphemur/SoundSwitch/issues/242) [#1176](https://github.com/Belphemur/SoundSwitch/issues/1176)

## [6.8.1](https://github.com/Belphemur/SoundSwitch/compare/v6.8.0...v6.8.1) (2023-12-24)


### Enhancements

* **installer:** add Korean language to the installer ([d30cdfd](https://github.com/Belphemur/SoundSwitch/commit/d30cdfd0f91489fb3395ca9263ead0fe2fded9ed))
* **profile::icon:** Check all device in profile for icon in systray menu ([f354698](https://github.com/Belphemur/SoundSwitch/commit/f3546986583dc60085414a65627491316817ad8e)), closes [#1332](https://github.com/Belphemur/SoundSwitch/issues/1332)
* **profile:** Order by alphabetical when showing profile in the systray menu ([58bec3f](https://github.com/Belphemur/SoundSwitch/commit/58bec3f2fea3ccf6bd28b4b7f52f311d134a18a4))


### Languages

* **Croatian:** Translated Settings using Weblate ([04bf70d](https://github.com/Belphemur/SoundSwitch/commit/04bf70de3f710892d7090643f07d56b50a6064a9))
* **Italian:** Translated Settings using Weblate ([d62ee5b](https://github.com/Belphemur/SoundSwitch/commit/d62ee5b5d2253a1553a8cd6d87f12f1bbfaa8dca))
* **Portuguese:** Translated Settings using Weblate ([ce245db](https://github.com/Belphemur/SoundSwitch/commit/ce245db1115b5e2b66ab230932c40e386fb98566))
* **Spanish:** Translated Settings using Weblate ([e156252](https://github.com/Belphemur/SoundSwitch/commit/e156252be3ea93f1e043e603cfb01f840a8352de))


### Bug Fixes

* **Startup:** Fix possible startup crash ([f841977](https://github.com/Belphemur/SoundSwitch/commit/f8419772624f8a4612746593bd8d75408524f2d2)), closes [#1341](https://github.com/Belphemur/SoundSwitch/issues/1341)

## [6.8.0](https://github.com/Belphemur/SoundSwitch/compare/v6.7.2...v6.8.0) (2023-12-19)


### Enhancements

* **.NET:** Move to .NET 8.0 ([43f0d60](https://github.com/Belphemur/SoundSwitch/commit/43f0d600ee98bb714294c81ed4962c84de947219))
* **device::cache:** Improve the performance and speed of caching devices ([594b49c](https://github.com/Belphemur/SoundSwitch/commit/594b49ca680b9582c02acd4a8ee9b865df3572fb))
* **device::cache:** Increase the TTL for refreshing device list ([6ab8028](https://github.com/Belphemur/SoundSwitch/commit/6ab8028c0e84642d9af995405a638d13c8417fe6))
* **device::cache:** keep only one cache alive for all devices ([9b3a126](https://github.com/Belphemur/SoundSwitch/commit/9b3a12657d4e527fe5366fa323b2a3f62ed644bb))
* **device::cache:** rework the logic behind refreshing devices when changes are detected in the system. ([3c12b3f](https://github.com/Belphemur/SoundSwitch/commit/3c12b3f1f87b6f253e45bc8b8786b9aa75f3c1a0))


### Languages

* **Bulgarian:** Translated Settings using Weblate ([b2cf464](https://github.com/Belphemur/SoundSwitch/commit/b2cf464a543546b71cb80ca16ed2a11c2c126ceb))
* **Bulgarian:** Translated Tray Icon using Weblate ([fffe534](https://github.com/Belphemur/SoundSwitch/commit/fffe534eeb24f2fb9335de2da672f6ccd8f6183d))
* **Chinese (Traditional):** Translated Settings using Weblate ([4582119](https://github.com/Belphemur/SoundSwitch/commit/45821192d165f5ab9498129367709cc30d9959e8))
* **French:** Translated Settings using Weblate ([4496335](https://github.com/Belphemur/SoundSwitch/commit/449633547683ccc4c8aeba85f5aaac180d597bcd))
* **French:** Translated Tray Icon using Weblate ([76905a4](https://github.com/Belphemur/SoundSwitch/commit/76905a462abf08384c212c4b0e51575e1f6d5d0b))
* **French:** Translated Update Download using Weblate ([67c281b](https://github.com/Belphemur/SoundSwitch/commit/67c281bae62c0e0710df0a5f6fe67707c62b672f))
* **German:** Translated Settings using Weblate ([061583c](https://github.com/Belphemur/SoundSwitch/commit/061583c95ef9f7b5170125a1e1b896fbbddb2dba))
* **German:** Translated Tray Icon using Weblate ([3da0c83](https://github.com/Belphemur/SoundSwitch/commit/3da0c83caa517fd781320b9a4b9ebf5daf0f85c8))
* **Korean:** Translated Tray Icon using Weblate ([4ca2b15](https://github.com/Belphemur/SoundSwitch/commit/4ca2b15c4a7c4efc115b007dc7e7df5f6a89013b))
* **Polish:** Translated Settings using Weblate ([43f8fa2](https://github.com/Belphemur/SoundSwitch/commit/43f8fa2a7dca5e8fc8a04b90c6fb7ed535a5ced7))
* **Polish:** Translated Tray Icon using Weblate ([3b05aeb](https://github.com/Belphemur/SoundSwitch/commit/3b05aeb37557089a7f7b0a5e29cde786a4793538))
* **Polish:** Translated Update Download using Weblate ([6cd0022](https://github.com/Belphemur/SoundSwitch/commit/6cd0022ebb483e8dbc61fa5ccc5cb55c2a479aa0))
* **Serbian:** Translated Tray Icon using Weblate ([d17e650](https://github.com/Belphemur/SoundSwitch/commit/d17e65077aebc4c1731ae5cf81cbd56e783a9e0f))
* **Slovenian:** Translated Tray Icon using Weblate ([b8c4269](https://github.com/Belphemur/SoundSwitch/commit/b8c4269a0dbf58cb7447c86501bdad1b8b6f8d3f))
* **Swedish:** Translated About using Weblate ([2b0a112](https://github.com/Belphemur/SoundSwitch/commit/2b0a112d45b5ba9269690fb4b7b2b8a7c59fe602))
* **Swedish:** Translated About using Weblate ([943befb](https://github.com/Belphemur/SoundSwitch/commit/943befb13e8a0ec9a7d8e5707c9727c06c26bf93))
* **Swedish:** Translated Settings using Weblate ([9a182a1](https://github.com/Belphemur/SoundSwitch/commit/9a182a1e7712c11008aa440a5d9089b23cd40305))
* **Swedish:** Translated Settings using Weblate ([49d3fcc](https://github.com/Belphemur/SoundSwitch/commit/49d3fccbc36288e265a2cded7d379e13a9e58f55))
* **Swedish:** Translated Settings using Weblate ([f2eb255](https://github.com/Belphemur/SoundSwitch/commit/f2eb255b2b9622de57cf6cb2ab2986904198ef29))
* **Swedish:** Translated Settings using Weblate ([d9e285f](https://github.com/Belphemur/SoundSwitch/commit/d9e285fb6c8990c958a66258b6bb0dfdebc06dde))
* **Swedish:** Translated Settings using Weblate ([3454648](https://github.com/Belphemur/SoundSwitch/commit/3454648677750551490b48b907b8fb5bbdbc2c68))
* **Swedish:** Translated Tray Icon using Weblate ([e939073](https://github.com/Belphemur/SoundSwitch/commit/e939073b62fc58285131be9fa65c278bd0824364))
* **Swedish:** Translated Tray Icon using Weblate ([fe21446](https://github.com/Belphemur/SoundSwitch/commit/fe2144620d63a61fd50cb11ac80e220f53688ec7))
* **Swedish:** Translated Update Download using Weblate ([c95fe6e](https://github.com/Belphemur/SoundSwitch/commit/c95fe6e8a2b298065992a5d9fe9fa3bf5daebd96))
* **Swedish:** Translated Update Download using Weblate ([1b65fa4](https://github.com/Belphemur/SoundSwitch/commit/1b65fa4a16a8d87957f24edc042419b33f001471))


### Bug Fixes

* **Device::Cache:** Devices not being refreshed ([a113175](https://github.com/Belphemur/SoundSwitch/commit/a1131753e06eb48d61ed93f872d95f9d1e734153)), closes [#1323](https://github.com/Belphemur/SoundSwitch/issues/1323)
* **profile:** possible issue with profile where the the state couldn't be restored after the profile ended. ([a648e4e](https://github.com/Belphemur/SoundSwitch/commit/a648e4e66e90e280f73131c09e02cde8e05c90f6))


### Features

* **volume:** Add an option to keep volume level across audio devices ([ca293b5](https://github.com/Belphemur/SoundSwitch/commit/ca293b529c420397db63ecdc03d2e295cbdc96ff)), closes [#1328](https://github.com/Belphemur/SoundSwitch/issues/1328)

## [6.7.2](https://github.com/Belphemur/SoundSwitch/compare/v6.7.1...v6.7.2) (2023-06-14)


### Bug Fixes

* **MMNotificationClient:** Fix possible crash when closing the app ([ab8c0ee](https://github.com/Belphemur/SoundSwitch/commit/ab8c0ee7d8d59470c67fdc40b44185cccc0a4cad)), closes [#1201](https://github.com/Belphemur/SoundSwitch/issues/1201)
* **Notification:** Fix missing sound in audio notification ([131c9d4](https://github.com/Belphemur/SoundSwitch/commit/131c9d42bec8ead3951123a12aa97b72e1afe3fd)), closes [#1203](https://github.com/Belphemur/SoundSwitch/issues/1203)


### Enhancements

* **Device::Refresh:** Wait a little longer before refreshing list of devices ([cb3d63f](https://github.com/Belphemur/SoundSwitch/commit/cb3d63f3c292939db5bcbebe7bc384f3b4758eea)), closes [#SOUNDSWITCH-151](https://github.com/Belphemur/SoundSwitch/issues/SOUNDSWITCH-151)


### Languages

* **Portuguese:** Translated Tray Icon using Weblate ([2e87f22](https://github.com/Belphemur/SoundSwitch/commit/2e87f220a76a4aa4254f004a5ef035c92b4e1d80))

## [6.7.1](https://github.com/Belphemur/SoundSwitch/compare/v6.7.0...v6.7.1) (2023-06-08)


### Bug Fixes

* **MMNotification:** Fix getting the current state of Default device at application startup. No more crash when there isn't a communication device set. ([e97330e](https://github.com/Belphemur/SoundSwitch/commit/e97330eb66c75108b900aa710ef234bc2219a709)), closes [#1200](https://github.com/Belphemur/SoundSwitch/issues/1200)

## [6.7.0](https://github.com/Belphemur/SoundSwitch/compare/v6.6.1...v6.7.0) (2023-06-08)


### Features

* **Core::Reset:** Add a mneu item to reset the Windows 10/11 per application audio settings. ([a5c07e8](https://github.com/Belphemur/SoundSwitch/commit/a5c07e83af729af04d2a675e6c45712c1b0e5dd5))
* **Core:** Replace core of the software by the CoreAudio library ([899058b](https://github.com/Belphemur/SoundSwitch/commit/899058b89a047bcdd6cae93279f731b4d353349b)), closes [#1184](https://github.com/Belphemur/SoundSwitch/issues/1184) [#1164](https://github.com/Belphemur/SoundSwitch/issues/1164) [#1175](https://github.com/Belphemur/SoundSwitch/issues/1175)


### Bug Fixes

* **Foreground:** Issue with Foreground application wrongly detected as SoundSwitch causing SoundSwitch to stop notifying when audio is changed. ([31e6fe1](https://github.com/Belphemur/SoundSwitch/commit/31e6fe1dc4e65a31ed4714b64854d4a2e99e10a6))


### Languages

* **Croatian:** Translated Settings using Weblate ([08c491a](https://github.com/Belphemur/SoundSwitch/commit/08c491a29790e8fe2886621b4b9629fd70dc4751))
* **Croatian:** Translated Tray Icon using Weblate ([dd1212b](https://github.com/Belphemur/SoundSwitch/commit/dd1212b20ecd5cf1c21f5e31d4a19df35cf74941))
* **Hebrew:** Translated Settings using Weblate ([a8cb593](https://github.com/Belphemur/SoundSwitch/commit/a8cb593801e51bdee50173bc0eb3458e6b5402b7))
* **Hebrew:** Translated Settings using Weblate ([325911d](https://github.com/Belphemur/SoundSwitch/commit/325911de272f97830c06dbdba62feb547f3bf99e))
* **Hebrew:** Translated Tray Icon using Weblate ([e715d44](https://github.com/Belphemur/SoundSwitch/commit/e715d444f370fa6f25fbee064d59c77645417d10))
* **Italian:** Translated Tray Icon using Weblate ([3ebd3ec](https://github.com/Belphemur/SoundSwitch/commit/3ebd3ec487e335da183bc86a24403965a5b44c67))
* **Portuguese (Brazil):** Translated Tray Icon using Weblate ([f6b406b](https://github.com/Belphemur/SoundSwitch/commit/f6b406b19155f86ddc89900232561437d093804d))
* **Spanish:** Translated Tray Icon using Weblate ([f0fd9e6](https://github.com/Belphemur/SoundSwitch/commit/f0fd9e662b8b8d5f925a94ef2a14eaca6a853509))

## [6.6.1](https://github.com/Belphemur/SoundSwitch/compare/v6.6.0...v6.6.1) (2023-04-17)


### Bug Fixes

* **Icon:** Fix icon not changing ([553b409](https://github.com/Belphemur/SoundSwitch/commit/553b409556378c52cb7e3c06d54e718ee3a49d1e)), closes [#1159](https://github.com/Belphemur/SoundSwitch/issues/1159)
* **Profile:** Disable profile from switching foreground app by default. ([f798580](https://github.com/Belphemur/SoundSwitch/commit/f798580106a95fab57bfa987eba406b4310b04f2))

## [6.6.0](https://github.com/Belphemur/SoundSwitch/compare/v6.5.4...v6.6.0) (2023-04-09)


### Features

* **Profile::Foreground:** Let profile be able to switch the foreground application. ([a63eac9](https://github.com/Belphemur/SoundSwitch/commit/a63eac9e4f05512dd6e0d500b4db6f4564ac3cd7)), closes [#1117](https://github.com/Belphemur/SoundSwitch/issues/1117)


### Bug Fixes

* **CustomSound:** Don't crash on invalid custom sound. ([5a0acf2](https://github.com/Belphemur/SoundSwitch/commit/5a0acf2d1d5ecce98896629c1118c1f46be874cc)), closes [#1130](https://github.com/Belphemur/SoundSwitch/issues/1130)
* **Icon:** Don't change icon when communication device is changed. ([84b7b46](https://github.com/Belphemur/SoundSwitch/commit/84b7b4674a53708b9b619ce5d7b0e73aec1c38a0)), closes [#1134](https://github.com/Belphemur/SoundSwitch/issues/1134)
* **Profile:** Not switching all devices ([b23fffc](https://github.com/Belphemur/SoundSwitch/commit/b23fffc7970269dcf0e1903bdcf131434e5bc33c)), closes [#1126](https://github.com/Belphemur/SoundSwitch/issues/1126)
* **Tooltip:** Fix not showing the right recording device in tooltip when SoundSwitch starts ([c2bd813](https://github.com/Belphemur/SoundSwitch/commit/c2bd813dc5006a83c0c15c3021122d81dcd80840)), closes [#1123](https://github.com/Belphemur/SoundSwitch/issues/1123)
* **Tooltip:** Fix not showing the right recording device in tooltip when SoundSwitch starts ([41ea9ac](https://github.com/Belphemur/SoundSwitch/commit/41ea9ace513c3dff8f39bf8aee23c63d14b57eaa)), closes [#1123](https://github.com/Belphemur/SoundSwitch/issues/1123)


### Languages

* **Arabic:** Translated Settings using Weblate ([0a76385](https://github.com/Belphemur/SoundSwitch/commit/0a76385f4fb3f8fc905482e5983eebaaa7a1231e))
* **Arabic:** Translated Update Download using Weblate ([2942b01](https://github.com/Belphemur/SoundSwitch/commit/2942b01f943c7a7ea0c21deeb476a7806708b24b))

## [6.5.4](https://github.com/Belphemur/SoundSwitch/compare/v6.5.3...v6.5.4) (2023-02-09)


### Bug Fixes

* **Device::Refresh:** Fix issue where the list of device wouldn't be properly refresh after new device added/removed. ([cbb7d25](https://github.com/Belphemur/SoundSwitch/commit/cbb7d25bd239d22c9ee4735c1c437964661119ef)), closes [#1113](https://github.com/Belphemur/SoundSwitch/issues/1113)


### Languages

* **Chinese (Simplified):** Translated Settings using Weblate ([fccb108](https://github.com/Belphemur/SoundSwitch/commit/fccb108a22d61d873dc9333017cb9944c08035e9))
* **Polish:** Translated Settings using Weblate ([be6880a](https://github.com/Belphemur/SoundSwitch/commit/be6880a6c2f622d8ffb909deaceed926f86046fc))
* **Serbian:** Translated Settings using Weblate ([2b70f3e](https://github.com/Belphemur/SoundSwitch/commit/2b70f3e59c671f608be864b50645a014fbdb31d1))

## [6.5.3](https://github.com/Belphemur/SoundSwitch/compare/v6.5.2...v6.5.3) (2023-02-05)


### Bug Fixes

* **Profile::ForceProfile:** Fix issue with force profile crashing SoundSwitch. Fix infinite loop. ([988448c](https://github.com/Belphemur/SoundSwitch/commit/988448cdddbbf9671e4b1eb9589ae070e670810d)), closes [#1103](https://github.com/Belphemur/SoundSwitch/issues/1103)


### Enhancements

* **Profile::Icon:** Always have an icon for the profile banner notification. Default to the first device of the profile when it's not linked to an application. ([c2f64d8](https://github.com/Belphemur/SoundSwitch/commit/c2f64d8638b9850492499f17774d8b070cc95da2)), closes [#1109](https://github.com/Belphemur/SoundSwitch/issues/1109)

## [6.5.2](https://github.com/Belphemur/SoundSwitch/compare/v6.5.1...v6.5.2) (2023-02-03)


### Bug Fixes

* **Profile::Communication:** Set properly the communication device from a profile. ([4a83ac5](https://github.com/Belphemur/SoundSwitch/commit/4a83ac5dd38709508af7b47ee0cd1e56bf99b9a8)), closes [#1104](https://github.com/Belphemur/SoundSwitch/issues/1104)

## [6.5.1](https://github.com/Belphemur/SoundSwitch/compare/v6.5.0...v6.5.1) (2023-02-02)


### Bug Fixes

* **Crash::NoError:** Fix crashing without error ([4d2ee17](https://github.com/Belphemur/SoundSwitch/commit/4d2ee17d15907d556df563085c046b76c176257b))
* **logging:** Make foreground change message verbose ([6c69344](https://github.com/Belphemur/SoundSwitch/commit/6c693441cd56d0b8834f636643e3957e00902b2d))
* **Notification::Banner:** Fix double notification (for both playback and recording device) in Win 11 ([a28b70d](https://github.com/Belphemur/SoundSwitch/commit/a28b70dd28caf8c51185aca6f9002b8e73bfa798))
* **Notification::Win11:** Fix double notification ([ed20fd5](https://github.com/Belphemur/SoundSwitch/commit/ed20fd5483570a9c1d4ac1f6da4c3eb86972463e)), closes [#893](https://github.com/Belphemur/SoundSwitch/issues/893)
* **Profile::Editing:** Fix inability to change the Notify for profile ([0b0cd0c](https://github.com/Belphemur/SoundSwitch/commit/0b0cd0cfea2fc6ce153f2bd3fe6a79b987706791)), closes [#1090](https://github.com/Belphemur/SoundSwitch/issues/1090)
* **Steam::BigPicture:** Improve detection of steam big picture ([2e7a2af](https://github.com/Belphemur/SoundSwitch/commit/2e7a2afde8891ee6d4319249be3394d13aafe5d0))
* **Tooltip::Volume:** Be sure the volume of the tooltip is properly shown ([8fc9cc4](https://github.com/Belphemur/SoundSwitch/commit/8fc9cc4ee65cdee4d7e823166a2304b7067755ab)), closes [#1096](https://github.com/Belphemur/SoundSwitch/issues/1096)
* **Tooltip::Volume:** Fix how to show the volume in the tooltip. Start with the volume then name of device. ([9b21dc8](https://github.com/Belphemur/SoundSwitch/commit/9b21dc81ddc3c58d3148203823a1152a5d9da671)), closes [#1096](https://github.com/Belphemur/SoundSwitch/issues/1096)
* **Volume:** Fix volume calculation ([9b692c8](https://github.com/Belphemur/SoundSwitch/commit/9b692c8f512ff0bde4c3eca0fdc85d4e15c9ec24)), closes [#1096](https://github.com/Belphemur/SoundSwitch/issues/1096)
* **Window::Name:** Fix possible crash when trying to get the name of a Window ([6b45a50](https://github.com/Belphemur/SoundSwitch/commit/6b45a50a3ae427e4a5caa5bc6b4128cd528c806e)), closes [#1095](https://github.com/Belphemur/SoundSwitch/issues/1095)


### Languages

* **Korean:** Translated Settings using Weblate ([ab0aab7](https://github.com/Belphemur/SoundSwitch/commit/ab0aab7cae43b01c424eb84a5eae537544b8abae))


### Enhancements

* **logs:** Improve sentry log message in case of crash ([b1b093e](https://github.com/Belphemur/SoundSwitch/commit/b1b093e22d1986ead4ace76d43a51d070d53ccd5))
* **logs:** Remove unneeded threadid ([8e73936](https://github.com/Belphemur/SoundSwitch/commit/8e7393682e66bde4af6a1622e6e1f84fe276e2ee))
* **Profile::Form:** Reduce the size of the form to edit/create profile ([7e8c42a](https://github.com/Belphemur/SoundSwitch/commit/7e8c42af1f54c1de24df5397b009750caa18185b)), closes [#1091](https://github.com/Belphemur/SoundSwitch/issues/1091)
* **Profile::Steam:** Improve the detection of Steam Big Picture mode in Windows 11 ([af955f2](https://github.com/Belphemur/SoundSwitch/commit/af955f2962ec3140c7096abd3bc663ff6f16eec0))

## [6.5.0](https://github.com/Belphemur/SoundSwitch/compare/v6.4.3...v6.5.0) (2023-01-07)


### Features

* **Profile::RecordingCommunication:** Add possibility to set communication device for recording device in Profile.cs ([2ef778b](https://github.com/Belphemur/SoundSwitch/commit/2ef778ba3293b06553463f2d22cdc251b5e84641)), closes [#793](https://github.com/Belphemur/SoundSwitch/issues/793)


### Bug Fixes

* **Device::Matching:** Fix device matching between deviceinfo and any that inherit from it ([7881f16](https://github.com/Belphemur/SoundSwitch/commit/7881f16aca706798a183ea601e0fd29c0de324f4))
* **Device::Selection:** Fix issue with recording and playback device having the same name not appearing in selected devices. ([f1aef5d](https://github.com/Belphemur/SoundSwitch/commit/f1aef5d92adb9982c06c9d936ddefb983543d0a4)), closes [#1070](https://github.com/Belphemur/SoundSwitch/issues/1070)
* **Profile::SteamBigPicture:** Fix detecting the new big picture of steam. ([cd7c31f](https://github.com/Belphemur/SoundSwitch/commit/cd7c31fd92ec72ae4fce6ad09dd58dd5fde1da16)), closes [#1074](https://github.com/Belphemur/SoundSwitch/issues/1074)
* **QuickMenu:** Fix resizing the quick menu when the number of items changes ([5b1eaec](https://github.com/Belphemur/SoundSwitch/commit/5b1eaece9466f1e09f3da0542b18868b2494aaaa)), closes [#1086](https://github.com/Belphemur/SoundSwitch/issues/1086)


### Languages

* **Bulgarian:** Add Bulgarian to the application ([74cce0d](https://github.com/Belphemur/SoundSwitch/commit/74cce0db9dc26f70fcfc51332d5d9b609b3491c3))
* **Bulgarian:** Added About translation using Weblate ([d1c3fb3](https://github.com/Belphemur/SoundSwitch/commit/d1c3fb3d1024ae7403cf22b9119049469ce091ec))
* **Bulgarian:** Added Settings translation using Weblate ([c05edfe](https://github.com/Belphemur/SoundSwitch/commit/c05edfe52611e7c6d8bf00aeb5a36a593b66cac3))
* **Bulgarian:** Added Tray Icon translation using Weblate ([e523b8c](https://github.com/Belphemur/SoundSwitch/commit/e523b8cad7fc37223c3c31ac09c2101fe8cd091a))
* **Bulgarian:** Added Update Download translation using Weblate ([4fdd134](https://github.com/Belphemur/SoundSwitch/commit/4fdd1340a0916e36c601ba8a176460b91750a460))
* **Bulgarian:** Translated About using Weblate ([8bfea2a](https://github.com/Belphemur/SoundSwitch/commit/8bfea2ab36a32668561b21868dac35945684ddc6))
* **Bulgarian:** Translated Settings using Weblate ([4f79f46](https://github.com/Belphemur/SoundSwitch/commit/4f79f461f5709b0e4ad9ad71cef5e329536fe6bb))
* **Bulgarian:** Translated Tray Icon using Weblate ([5f2fc01](https://github.com/Belphemur/SoundSwitch/commit/5f2fc012193338d72eb4bc0effa98b3434a9dc61))
* **Bulgarian:** Translated Update Download using Weblate ([6b05d44](https://github.com/Belphemur/SoundSwitch/commit/6b05d44f7758f4e0f42b7bf8c856085b51abab9a))
* **Bulgarian:** Translated Update Download using Weblate ([f587f06](https://github.com/Belphemur/SoundSwitch/commit/f587f066e4212f37154012df9d08089407bd3ae8))
* **Ukrainian:** Add Ukrainian to the list of supported languages ([a3684d3](https://github.com/Belphemur/SoundSwitch/commit/a3684d323f7e01e1b157658e2e9481be516d8c82))
* **Ukrainian:** Added About translation using Weblate ([e584982](https://github.com/Belphemur/SoundSwitch/commit/e584982781a2ae6f7f79aaf6c4eed911eb5c7c8d))
* **Ukrainian:** Added Settings translation using Weblate ([9490b89](https://github.com/Belphemur/SoundSwitch/commit/9490b89ecfe4e21f9a7abedda0a8a53457e1b171))
* **Ukrainian:** Added Tray Icon translation using Weblate ([b57f6e0](https://github.com/Belphemur/SoundSwitch/commit/b57f6e00b6363ffb6f7dd1c5488c5c98a51dfcfd))
* **Ukrainian:** Added Update Download translation using Weblate ([e64d78e](https://github.com/Belphemur/SoundSwitch/commit/e64d78e39f44a2d65e066debe141ae80a9bdb814))
* **Ukrainian:** Translated About using Weblate ([cc7ac86](https://github.com/Belphemur/SoundSwitch/commit/cc7ac86e221efad0daa8694232c4b8a916bf5b54))
* **Ukrainian:** Translated Settings using Weblate ([edb8ab4](https://github.com/Belphemur/SoundSwitch/commit/edb8ab497230caa4aadcefc2c6f3d289d41b00f1))
* **Ukrainian:** Translated Settings using Weblate ([c50c67e](https://github.com/Belphemur/SoundSwitch/commit/c50c67ead79eb4ae74d85778235692d78078c7b8))
* **Ukrainian:** Translated Tray Icon using Weblate ([d82d954](https://github.com/Belphemur/SoundSwitch/commit/d82d95489ea079c88c9149029252397b72f6ce96))
* **Ukrainian:** Translated Tray Icon using Weblate ([0250383](https://github.com/Belphemur/SoundSwitch/commit/02503832f10ca76ecefcd196e2c8fc4783370a17))
* **Ukrainian:** Translated Update Download using Weblate ([0c80e2e](https://github.com/Belphemur/SoundSwitch/commit/0c80e2e5384e04757bcc9d469aa2896657e9d9d7))

## [6.4.3](https://github.com/Belphemur/SoundSwitch/compare/v6.4.2...v6.4.3) (2022-11-18)


### Bug Fixes

* **Hotkey:** Fix hotkey not being registered when computer comes back from sleep ([ed05d62](https://github.com/Belphemur/SoundSwitch/commit/ed05d624bf1af1ab51e6cc46dc9971587baa219b)), closes [#1041](https://github.com/Belphemur/SoundSwitch/issues/1041) [#997](https://github.com/Belphemur/SoundSwitch/issues/997)


### Languages

* Add danish to the application ([1d5ff6b](https://github.com/Belphemur/SoundSwitch/commit/1d5ff6b4a49386e12f4eebca079ea5b3c1dc3ccb))
* **Croatian:** Translated Settings using Weblate ([63bf492](https://github.com/Belphemur/SoundSwitch/commit/63bf4927367257a834e3641abaef29ae7aada8a8))
* **Danish:** Added About translation using Weblate ([76221bf](https://github.com/Belphemur/SoundSwitch/commit/76221bf35af224937e7bbe8b55703279685517f9))
* **Danish:** Translated About using Weblate ([b0b9ddd](https://github.com/Belphemur/SoundSwitch/commit/b0b9dddf6d87f8e3336421fd0ba7d63e9b300e16))
* **Danish:** Translated Settings using Weblate ([cfa248f](https://github.com/Belphemur/SoundSwitch/commit/cfa248fd7d0a3a7c6bd54193e187ae8c80f9b804))
* **German:** Translated Settings using Weblate ([39a2e80](https://github.com/Belphemur/SoundSwitch/commit/39a2e807d18df80fdf88cc21020b2fe6cfa4717a))
* **German:** Translated Settings using Weblate ([76d4691](https://github.com/Belphemur/SoundSwitch/commit/76d469153c52732459264d0960c28aa0baa361ec))
* **German:** Translated Update Download using Weblate ([5e8d91b](https://github.com/Belphemur/SoundSwitch/commit/5e8d91b1af2bc46ddec9f165cc437ab0c7e0f52b))
* **Spanish:** Translated Settings using Weblate ([25f9ab9](https://github.com/Belphemur/SoundSwitch/commit/25f9ab9b88a811126f7d87f3c92eba8371604e80))


### Enhancements

* **.NET:** Update to .NET 7.0 ([72cb6b8](https://github.com/Belphemur/SoundSwitch/commit/72cb6b841a0d815a5efe131ddfb800cfc99b040b))

## [6.4.2](https://github.com/Belphemur/SoundSwitch/compare/v6.4.1...v6.4.2) (2022-08-09)


### Bug Fixes

* **Collection:** Remove properly device when unselected from the list. ([2f70eb2](https://github.com/Belphemur/SoundSwitch/commit/2f70eb2824852ff9012842bfa9b5ac42a02dd151)), closes [#978](https://github.com/Belphemur/SoundSwitch/issues/978)
* **Win11::RoundedCorner:** Fix possible crash when showing menu and trying to apply the rounding. ([d6050e4](https://github.com/Belphemur/SoundSwitch/commit/d6050e4404141d3dfb42a346ec6a8c5c87a7612a)), closes [#977](https://github.com/Belphemur/SoundSwitch/issues/977)


### Languages

* **Slovenian:** Translated Settings using Weblate ([4dad868](https://github.com/Belphemur/SoundSwitch/commit/4dad86837a7ea4def6dd109024bb132aa792ac2c))
* **Slovenian:** Translated Tray Icon using Weblate ([0fe7a50](https://github.com/Belphemur/SoundSwitch/commit/0fe7a50166defbc9f96a537323f672790d40d5da))
* **Slovenian:** Translated Update Download using Weblate ([06f8b44](https://github.com/Belphemur/SoundSwitch/commit/06f8b4444f9b540b5bd299f4d5042e864eb2d9ad))

## [6.4.1](https://github.com/Belphemur/SoundSwitch/compare/v6.4.0...v6.4.1) (2022-08-03)


### Bug Fixes

* **AutoAdd:** Only trigger the auto add when a new device is actually added ([fe26910](https://github.com/Belphemur/SoundSwitch/commit/fe2691063218d8d7032ff691dbe5b863c65cb8b3))
* **Crash:** Fix crash when crashing. Be sure we can get the message and tell the user to report the issue. ([8303e30](https://github.com/Belphemur/SoundSwitch/commit/8303e302c7241dd9771de9c14b3583b5d9bf5f65))
* **lang:** Chinese installer language ([0b3203f](https://github.com/Belphemur/SoundSwitch/commit/0b3203f79860694912377cde75bbea0739081918))


### Languages

* **Japanese:** Translated Settings using Weblate ([87daa76](https://github.com/Belphemur/SoundSwitch/commit/87daa7611d4cab958181ee1746d6b84a3739978e))


### Enhancements

* **quickmenu:** Make quick menu rounded like banner ([fd421aa](https://github.com/Belphemur/SoundSwitch/commit/fd421aa382890fd8b98074d169734da4a90a0f54))
* **Rounded:** Add rounded corner to the banner and menus ([4358c88](https://github.com/Belphemur/SoundSwitch/commit/4358c883904b2bd142c1667cd0482aad9c2f532c)), closes [#971](https://github.com/Belphemur/SoundSwitch/issues/971)

## [6.4.0](https://github.com/Belphemur/SoundSwitch/compare/v6.3.0...v6.4.0) (2022-07-30)


### Features

* **AutoAdd:** Auto select new device when they are connected and SoundSwitch is running ([a463873](https://github.com/Belphemur/SoundSwitch/commit/a463873b21eb1015d0cb4ec27c4294986c459c36)), closes [#924](https://github.com/Belphemur/SoundSwitch/issues/924)


### Bug Fixes

* **Banner:** Avoid issue where the banner wouldn't be shown ([c6e1c6b](https://github.com/Belphemur/SoundSwitch/commit/c6e1c6b59fd55b017a4c779c2dc58c2d88d9480b)), closes [#960](https://github.com/Belphemur/SoundSwitch/issues/960)
* **DeviceCollection:** Fix possible issue where we wouldn't import the full list of selected devices ([a78377c](https://github.com/Belphemur/SoundSwitch/commit/a78377c01f48c33b3fcedf80e46b45673a534765))


### Enhancements

* **Autoupdate:** improve logged error message when signature check fails ([e017bc5](https://github.com/Belphemur/SoundSwitch/commit/e017bc54974bbe635a3b1b5580a2f5f1cb9eb410))
* **DeviceCollection:** Return device order by discovery ([0154286](https://github.com/Belphemur/SoundSwitch/commit/015428669b844c62838f64af1448e5e0d54d1625))
* **QuickMenu:** Don't enable quick menu by default for new users. ([94327e7](https://github.com/Belphemur/SoundSwitch/commit/94327e76d804d50eb0fb683fa9c989328dcb4c77))


### Languages

* **Croatian:** Translated Settings using Weblate ([f6cff4c](https://github.com/Belphemur/SoundSwitch/commit/f6cff4c1686dc644beba974553743d42a30e4de1))
* **Hebrew:** Translated Settings using Weblate ([64a9a91](https://github.com/Belphemur/SoundSwitch/commit/64a9a91f4f7377829a7347bfa4ec1f698d2038fa))
* **Italian:** Translated Settings using Weblate ([b35e0e1](https://github.com/Belphemur/SoundSwitch/commit/b35e0e1c693926a690dbb89694c3d45ccd5fa779))
* **Japanese:** Translated Settings using Weblate ([f0b1cbd](https://github.com/Belphemur/SoundSwitch/commit/f0b1cbdc6dcfe63a1f61716aaeffab068130e89e))
* **Japanese:** Translated Settings using Weblate ([211e299](https://github.com/Belphemur/SoundSwitch/commit/211e299f7d494ecae5c3c19fcd11758465de6ee2))
* **Korean:** Translated Settings using Weblate ([0671498](https://github.com/Belphemur/SoundSwitch/commit/0671498bbc02e372a26b503a848e3a1b82781cf3))
* **Portuguese (Brazil):** Translated Settings using Weblate ([3ab4cd6](https://github.com/Belphemur/SoundSwitch/commit/3ab4cd6795d4e53f458d8823ce14fb6cd74dff9f))
* **Portuguese:** Translated Settings using Weblate ([d899f50](https://github.com/Belphemur/SoundSwitch/commit/d899f50d3d10e163a450a33c9ec28df206a6a331))
* **Portuguese:** Translated Settings using Weblate ([25e0409](https://github.com/Belphemur/SoundSwitch/commit/25e0409103e1e70ad016ddcbf83fbf68c393836b))
* **Thai:** Translated Settings using Weblate ([b724e0e](https://github.com/Belphemur/SoundSwitch/commit/b724e0e8f59d1258ef849345cf40592127797da2))

## [6.3.0](https://github.com/Belphemur/SoundSwitch/compare/v6.2.4...v6.3.0) (2022-06-26)


### Features

* **Profile::Force:** Add trigger for a profile to force that profile to be applied anytime the profile sound device is changed. ([753f0a3](https://github.com/Belphemur/SoundSwitch/commit/753f0a32c9af73274685c94b7749828e4d540458)), closes [#648](https://github.com/Belphemur/SoundSwitch/issues/648)


### Bug Fixes

* **AudioSwitching:** Fix issue for Windows 10 <= 1709 where the new audio API aren't available ([ad66ec0](https://github.com/Belphemur/SoundSwitch/commit/ad66ec0066d02bfce32a144b44b35eab1caf62e9))
* **AudioSwitching:** Fix issue with some version of windows 10 that couldn't switch audio of specific application ([76929b6](https://github.com/Belphemur/SoundSwitch/commit/76929b6164e32cb92cc90ea8ceaf379349a67092))
* **Device::Name:** Fix the regex used to clean the name of device to not match custom naming. ([3e7b27e](https://github.com/Belphemur/SoundSwitch/commit/3e7b27eb3a634fa87c31d192060e96ca6714eb90)), closes [#909](https://github.com/Belphemur/SoundSwitch/issues/909)
* **Device:** Fix issue with device changing name, as long as the id is the same, SoundSwitch will find the device. ([1bd6c28](https://github.com/Belphemur/SoundSwitch/commit/1bd6c28c70502d2e3a14deee98db2474ebb4849d)), closes [#868](https://github.com/Belphemur/SoundSwitch/issues/868)
* **Device:** Fix issue with device having the same name not being show as different device in the UI ([2249ed0](https://github.com/Belphemur/SoundSwitch/commit/2249ed0e2d053644bb2d781603cc7023bc79732d))
* **Sentry:** Fix spamming sentry with error that can't be fixed ([a74a76f](https://github.com/Belphemur/SoundSwitch/commit/a74a76f238f68d2065f257dfdd4b047ba08ee4b9))
* **Switching:** Possible crash when trying to switch audio of process that just closed. ([d4894a0](https://github.com/Belphemur/SoundSwitch/commit/d4894a0df1e965bf5c9c0e7b5caab1cfe592367b))


### Languages

* **Croatian:** Translated Settings using Weblate ([daa498a](https://github.com/Belphemur/SoundSwitch/commit/daa498a5cc491f555898365ebdea07ed6a27ea44))
* **German:** Translated Settings using Weblate ([e177bf8](https://github.com/Belphemur/SoundSwitch/commit/e177bf89b14dcc0d223b5b6722766938580333e9))
* **German:** Translated Settings using Weblate ([6c680db](https://github.com/Belphemur/SoundSwitch/commit/6c680db7535433db316e3c305f98f619ace74d86))
* **German:** Translated Update Download using Weblate ([ba12587](https://github.com/Belphemur/SoundSwitch/commit/ba12587b2aff2765273e0d5569259355c69fd9c6))
* **Hebrew:** Translated Settings using Weblate ([1918b35](https://github.com/Belphemur/SoundSwitch/commit/1918b3575c1366551851534c6efa1fe7104c462f))
* **Italian:** Translated Settings using Weblate ([ff62704](https://github.com/Belphemur/SoundSwitch/commit/ff62704380033343da7585080d5817a3a6557c10))
* **Italian:** Translated Tray Icon using Weblate ([c417b11](https://github.com/Belphemur/SoundSwitch/commit/c417b111c78ce76f15b60768f42fe8b05fe16625))
* **Italian:** Translated Tray Icon using Weblate ([6054428](https://github.com/Belphemur/SoundSwitch/commit/6054428060c25d2f0537f66005de18d3e5401829))
* **Korean:** Translated Update Download using Weblate ([94411c7](https://github.com/Belphemur/SoundSwitch/commit/94411c7dea0c11696c6ecdf4ae5ad8251e86b20d))
* **Norwegian Bokmål:** Translated Update Download using Weblate ([ca103fe](https://github.com/Belphemur/SoundSwitch/commit/ca103fe8d8bfc241898b381726722870e195643d))
* **Portuguese (Brazil):** Translated Settings using Weblate ([9984be5](https://github.com/Belphemur/SoundSwitch/commit/9984be5cfad175514551a60d781ad86b76f3d962))


### Enhancements

* **AutoUpdate:** Keep the Update in a Temp folder with the real name of the file. Let windows clean them up later. ([59274db](https://github.com/Belphemur/SoundSwitch/commit/59274db95a41db3d13084733d3d62959804a3163))
* **Configuration:** Cleanup configuration of any duplicated device by name clean ([3826e9b](https://github.com/Belphemur/SoundSwitch/commit/3826e9bd2c38334abd51c782d929083f53ba3d71)), closes [#943](https://github.com/Belphemur/SoundSwitch/issues/943)

### [6.2.4](https://github.com/Belphemur/SoundSwitch/compare/v6.2.3...v6.2.4) (2021-12-18)


### Bug Fixes

* **Device:** Rely completely on the name of the device for it's uniqueness in the selection of devices. ([d5bd9e8](https://github.com/Belphemur/SoundSwitch/commit/d5bd9e869ffde33ac6c1f59a3cbd0feff0b76827))


### Languages

* **Czech:** Translated Settings using Weblate ([bc6a46d](https://github.com/Belphemur/SoundSwitch/commit/bc6a46d743ca54b10384d44223c74d720fd044e9))
* **Czech:** Translated Tray Icon using Weblate ([412ea5a](https://github.com/Belphemur/SoundSwitch/commit/412ea5a897ea2d70204cd92d2e9205aafc2b6301))
* **Czech:** Translated Update Download using Weblate ([1235d2c](https://github.com/Belphemur/SoundSwitch/commit/1235d2c4a227eda2c8b83ca342e630fa2f048c43))

### [6.2.3](https://github.com/Belphemur/SoundSwitch/compare/v6.2.2...v6.2.3) (2021-11-05)


### Bug Fixes

* **AudioSwitcher:** Switching audio for Windows 10 < 1803 ([8b0f4c9](https://github.com/Belphemur/SoundSwitch/commit/8b0f4c90478f02772114a133227199cbfa5dd4f2))
* **AudioSwitcher:** Switching audio for Windows 10 < 1803 ([e30e23e](https://github.com/Belphemur/SoundSwitch/commit/e30e23e975e313ae82983c6452ca4c115dcbf5c8))
* **QuickMenu:** Take into account the edge of the screen and show the quick menu properly ([bc9eccb](https://github.com/Belphemur/SoundSwitch/commit/bc9eccbcc29677fbd482821039da4b60e8e352ed)), closes [#735](https://github.com/Belphemur/SoundSwitch/issues/735)
* **Update:** Only notify about the latest version, not all version higher than the current one ([337ea6f](https://github.com/Belphemur/SoundSwitch/commit/337ea6f9a0e122b6f421c5f0b784933eda39b822)), closes [#806](https://github.com/Belphemur/SoundSwitch/issues/806)

### [6.2.2](https://github.com/Belphemur/SoundSwitch/compare/v6.2.1...v6.2.2) (2021-10-25)


### Bug Fixes

* **AudioSwitcher:** Fix switching audio device on Windows 11 with "Also foreground app" ([ed6b92a](https://github.com/Belphemur/SoundSwitch/commit/ed6b92a305d16e46938e2e5465c46debf0d989e5)), closes [#802](https://github.com/Belphemur/SoundSwitch/issues/802)

### [6.2.1](https://github.com/Belphemur/SoundSwitch/compare/v6.2.0...v6.2.1) (2021-10-24)


### Bug Fixes

* **AudioSwitch:** Don't log crashed when trying to switch application audio endpoint on Windows 7 ([eb0d986](https://github.com/Belphemur/SoundSwitch/commit/eb0d9869f234ad01bc1761eacf780243f76bf141))
* **AudioSwitching:** Fix audio switching on Windows 11 ([40f5ba1](https://github.com/Belphemur/SoundSwitch/commit/40f5ba119a41d04b8057e75146345c84dfe238c3)), closes [#799](https://github.com/Belphemur/SoundSwitch/issues/799)

## [6.2.0](https://github.com/Belphemur/SoundSwitch/compare/v6.1.2...v6.2.0) (2021-10-23)


### Features

* **AudioSwitching:** Add support for Windows 11 ([b2e1de5](https://github.com/Belphemur/SoundSwitch/commit/b2e1de5de134dd264d2e2d9d6c2f2e4cad1231d8)), closes [#797](https://github.com/Belphemur/SoundSwitch/issues/797) [#548](https://github.com/Belphemur/SoundSwitch/issues/548)


### Bug Fixes

* **AudioSwitcher::Windows11:** Fix possible crash when using profile with application on Windows 11 ([8531ee9](https://github.com/Belphemur/SoundSwitch/commit/8531ee93c325deff4ab2b799b3052afd7ef92a65))
* **AudioSwitcher:** Properly use the right audio client for Windows post 21H2 (like Windows 11) ([7324b15](https://github.com/Belphemur/SoundSwitch/commit/7324b15e4d4cde859211b44cc1a95ddce90f1a97))
* **DeviceLister:** Fix crash when device lister is taking too long ([e7efb5c](https://github.com/Belphemur/SoundSwitch/commit/e7efb5c6cc94c3e1f205098eed3c3babab394803))
* **Notification::Sound:** Fix crash when playing a custom sound. ([f0a7113](https://github.com/Belphemur/SoundSwitch/commit/f0a71138cb0fd76559f284da98c5c75e880b407b)), closes [#790](https://github.com/Belphemur/SoundSwitch/issues/790)
* **Profile:** Fix systray icon not changing properly when using a profile ([aef15b7](https://github.com/Belphemur/SoundSwitch/commit/aef15b7386d1239bfbf40f10f29f90f3ca18abc2)), closes [#450](https://github.com/Belphemur/SoundSwitch/issues/450)


### Languages

* **Russian:** Translated Settings using Weblate ([24cb961](https://github.com/Belphemur/SoundSwitch/commit/24cb961bdc46e00e209e44e53bab444d72d949f1))


### Enhancements

* **AudioSwitcher:** Improve the logging of the switching for Windows 11 ([5e0a985](https://github.com/Belphemur/SoundSwitch/commit/5e0a985b59a63cd6225b5583ad5659707b9c22eb))
* **Mute:** Change the unmute icon ([844ad5f](https://github.com/Belphemur/SoundSwitch/commit/844ad5f619c09c80b538d6d3b5fabf130a5cb998)), closes [#791](https://github.com/Belphemur/SoundSwitch/issues/791)
* **Mute:** Improve the image to have the same size as device icon ([a840c82](https://github.com/Belphemur/SoundSwitch/commit/a840c82f678af4d37befcc340b62635fbfe83b29)), closes [#791](https://github.com/Belphemur/SoundSwitch/issues/791)
* **TrayIcon:** Add better logs to understand what's happening with the systray icon not changing. ([99d6243](https://github.com/Belphemur/SoundSwitch/commit/99d62436a038a9b0eb8e67ab80ddd9dd20b75d7d)), closes [#450](https://github.com/Belphemur/SoundSwitch/issues/450)

### [6.1.2](https://github.com/Belphemur/SoundSwitch/compare/v6.1.1...v6.1.2) (2021-10-13)


### Bug Fixes

* **Updater:** Be sure we can download the new releases ([1077dd2](https://github.com/Belphemur/SoundSwitch/commit/1077dd2b45e0b402b74166b051f7766b630be928))

### [6.1.1](https://github.com/Belphemur/SoundSwitch/compare/v6.1.0...v6.1.1) (2021-10-13)


### Bug Fixes

* **.NET::Build:** Fix issue with warnings about API not available ([987043e](https://github.com/Belphemur/SoundSwitch/commit/987043e377806ca4f1ab8c7b0b816c4681bbac5d))
* **CrashReport:** Fix message box not showing when application crashes ([6179c53](https://github.com/Belphemur/SoundSwitch/commit/6179c53b31c17b775478a29c8855af614c108ded))
* **HotKey:** Remove detection of any Hanja/Kanji mode key ([2ab20ea](https://github.com/Belphemur/SoundSwitch/commit/2ab20eadf836da2cdd326e17526986caeb668927))
* **IconChanger:** Fix issue where the icon wouldn't change when the default device is switched. ([b33ef84](https://github.com/Belphemur/SoundSwitch/commit/b33ef848b80252e3b1e132da2e61c1e909adc931))
* **Notification::CustomSound:** Fix issue loading MP3 file for custom sound. ([a46acb4](https://github.com/Belphemur/SoundSwitch/commit/a46acb448fa7520fe83cb73b6c6fd93520e1c7a1))
* **Profile::Validation:** Be sure we're not creating/updating a profile with the exact same trigger ([d1312c3](https://github.com/Belphemur/SoundSwitch/commit/d1312c38e764e80eef78fe5d3488f70114b1257a)), closes [#753](https://github.com/Belphemur/SoundSwitch/issues/753)
* **Profile:** Properly return issue when loading profile at application startup. ([6b27786](https://github.com/Belphemur/SoundSwitch/commit/6b277869b516ee68fa47d4e0462f21e0b44d7dd8))
* **TrayIcon:** Fix rare time where changing the icon make the application crash ([ae4f30b](https://github.com/Belphemur/SoundSwitch/commit/ae4f30b24e6e02f0057992b7f52fc9a748a681b3))


### Languages

* **Chinese (Simplified):** Translated Settings using Weblate ([592cca5](https://github.com/Belphemur/SoundSwitch/commit/592cca5b1cdc7d69c9c6502f957555ba7fea4bd0))
* **Chinese (Simplified):** Translated Update Download using Weblate ([4f7b24d](https://github.com/Belphemur/SoundSwitch/commit/4f7b24d2b606056ca2ad8439845f315527390857))
* **Croatian:** Translated Settings using Weblate ([cb560a0](https://github.com/Belphemur/SoundSwitch/commit/cb560a085ddb3003e1789853c633b26dbd998e0f))
* **Croatian:** Translated Update Download using Weblate ([9678a7f](https://github.com/Belphemur/SoundSwitch/commit/9678a7f4b48d8eea8be213c20be7198a993a18d0))
* **Italian:** Translated Settings using Weblate ([5888ba1](https://github.com/Belphemur/SoundSwitch/commit/5888ba18062305b4dd99443973b59b035ddf543c))
* **Italian:** Translated Update Download using Weblate ([a4589fe](https://github.com/Belphemur/SoundSwitch/commit/a4589fed26b1262cf7ef5680923fe3b769afc6ef))
* **Japanese:** Translated About using Weblate ([7e72027](https://github.com/Belphemur/SoundSwitch/commit/7e72027f1a6fbb369791a87c24ae2effd6058985))
* **Japanese:** Translated Settings using Weblate ([fdf4d2e](https://github.com/Belphemur/SoundSwitch/commit/fdf4d2e25c2eb7021360cefb5e1d26a5ceae1913))
* **Japanese:** Translated Update Download using Weblate ([321a886](https://github.com/Belphemur/SoundSwitch/commit/321a886d1c9423552046d7d3ed1f412b987bc515))
* **Korean:** Translated Settings using Weblate ([083b228](https://github.com/Belphemur/SoundSwitch/commit/083b228d0efd057d948e1271570294aff4c8c1c5))
* **Portuguese:** Translated Settings using Weblate ([b934427](https://github.com/Belphemur/SoundSwitch/commit/b9344276a826953ff5eeafb2d728818e9143b9ca))
* **Russian:** Translated Update Download using Weblate ([c8e9840](https://github.com/Belphemur/SoundSwitch/commit/c8e9840bc86f6748ec6f85bca9b1d7342f4ea456))
* **Serbian:** Add serbian to the application ([5d821f7](https://github.com/Belphemur/SoundSwitch/commit/5d821f7fc025d2ec756a66ba2fbd803c16226b15))
* **Serbian:** Added About translation using Weblate ([357c9a8](https://github.com/Belphemur/SoundSwitch/commit/357c9a807defe6d9318c0a8daa3f74cc3a5c46c3))
* **Serbian:** Added Settings translation using Weblate ([d718bf5](https://github.com/Belphemur/SoundSwitch/commit/d718bf5f0a5e7026d80d0673c019e933fa5a56f9))
* **Serbian:** Added Tray Icon translation using Weblate ([b647a94](https://github.com/Belphemur/SoundSwitch/commit/b647a949189440786be03686f4c961d27deab68e))
* **Serbian:** Added Update Download translation using Weblate ([fc30f7e](https://github.com/Belphemur/SoundSwitch/commit/fc30f7e8c506f73c0b7817fe2be38b57f6c82025))
* **Serbian:** Translated About using Weblate ([ceb39e8](https://github.com/Belphemur/SoundSwitch/commit/ceb39e802ccb3b5c87ba57868cc4fe5bdcb66bf8))
* **Serbian:** Translated Settings using Weblate ([b346d64](https://github.com/Belphemur/SoundSwitch/commit/b346d645ef15be954e44dcccb99a4744bfeab81e))
* **Serbian:** Translated Settings using Weblate ([6b4bc16](https://github.com/Belphemur/SoundSwitch/commit/6b4bc1605b34c999c3974ae8e6bc09aefacd6398))
* **Serbian:** Translated Tray Icon using Weblate ([49c6a45](https://github.com/Belphemur/SoundSwitch/commit/49c6a45c5b0771684ff91a07fbfba9f64166af73))
* **Serbian:** Translated Update Download using Weblate ([3557566](https://github.com/Belphemur/SoundSwitch/commit/3557566496e927bcb50603d5eec0baef498d2fa7))
* **Spanish:** Translated Settings using Weblate ([2fdaaf3](https://github.com/Belphemur/SoundSwitch/commit/2fdaaf33362719c9ffe1b755c1eb1b03b15284b6))
* **Spanish:** Translated Tray Icon using Weblate ([0cd5be4](https://github.com/Belphemur/SoundSwitch/commit/0cd5be49b3f3a2ad1ad25488bec0c6f19e17e532))
* **Spanish:** Translated Update Download using Weblate ([d99a52b](https://github.com/Belphemur/SoundSwitch/commit/d99a52b4cac01b44de4118a666f4b6ee87115367))
* **Thai:** Add language thai to the application ([57bc75a](https://github.com/Belphemur/SoundSwitch/commit/57bc75a8426fb3111b14f8bd22624c9dce662097))
* **Thai:** Added About translation using Weblate ([7f79dce](https://github.com/Belphemur/SoundSwitch/commit/7f79dce3fdaa37bd01f9d50639015af5c29808fa))
* **Thai:** Added Settings translation using Weblate ([750086c](https://github.com/Belphemur/SoundSwitch/commit/750086c10da19f91fe8da1421cc150ce61b6b1db))
* **Thai:** Added Tray Icon translation using Weblate ([ca24957](https://github.com/Belphemur/SoundSwitch/commit/ca24957e8c3555be29c0fc23953d08fbd434f277))
* **Thai:** Added Update Download translation using Weblate ([b7738ea](https://github.com/Belphemur/SoundSwitch/commit/b7738eacef888d7060ae68e288335e61ee426035))
* **Thai:** Translated About using Weblate ([3b135e3](https://github.com/Belphemur/SoundSwitch/commit/3b135e33f0156b45b691ee7ce42052b1321c1dff))
* **Thai:** Translated About using Weblate ([7fb4fd0](https://github.com/Belphemur/SoundSwitch/commit/7fb4fd050619d3e72d37429c47394f6ab4364109))
* **Thai:** Translated Settings using Weblate ([bd80ec6](https://github.com/Belphemur/SoundSwitch/commit/bd80ec6c4c9bca9c5504ee0ce7efed084a5a1146))
* **Thai:** Translated Settings using Weblate ([4b902da](https://github.com/Belphemur/SoundSwitch/commit/4b902da8d0c7b2f3f1a91e7c38428f62a14614aa))
* **Thai:** Translated Settings using Weblate ([02833e1](https://github.com/Belphemur/SoundSwitch/commit/02833e10de0e48b50ec66ccd4c81fe61d94638e0))
* **Thai:** Translated Tray Icon using Weblate ([a25c8c7](https://github.com/Belphemur/SoundSwitch/commit/a25c8c7a1dc7a360efda6d5461f63c6b56bb5311))
* **Thai:** Translated Update Download using Weblate ([a929093](https://github.com/Belphemur/SoundSwitch/commit/a929093bbf9cf7cdbb2e4a120dc48730c1bc6eff))


### Enhancements

* **.NET:** Move to .NET 6.0 ([4d1fb42](https://github.com/Belphemur/SoundSwitch/commit/4d1fb4286875d5719fc4106de1cd3126ee2d31de))
* **Logging:** Improve logging Switch foreground feature for better debugging ([5cfdf74](https://github.com/Belphemur/SoundSwitch/commit/5cfdf7442911c35712bb9b5aa9a5a40ab4807556))
* **Profile::QuickMenu:** Only display quick menu when selected in the global setting when switching between profiles. ([1e034ae](https://github.com/Belphemur/SoundSwitch/commit/1e034ae2feb7003ec8bd135213bc340d161c3c26)), closes [#745](https://github.com/Belphemur/SoundSwitch/issues/745)

## [6.1.0](https://github.com/Belphemur/SoundSwitch/compare/v6.0.0...v6.1.0) (2021-09-05)


### Features

* **Profile::Hotkey:** Profile can now share the same hotkey. Doing so let you switch between profile. A quick menu is also displayed. ([85a623e](https://github.com/Belphemur/SoundSwitch/commit/85a623e10c4c41657a99bbeedd9a6b8c0680d126)), closes [#409](https://github.com/Belphemur/SoundSwitch/issues/409)


### Bug Fixes

* **.NET:** Disable trimmming. It's recommended to avoid it when having a WinForm application. This will fixes a lot of startup crashes. ([033b968](https://github.com/Belphemur/SoundSwitch/commit/033b968bc459eaaec74acf1fc4824c5c21d68de1))
* **AutoUpdate:** Show a message box when can't validate signature of file. ([2406d6c](https://github.com/Belphemur/SoundSwitch/commit/2406d6cc976138edff8cfee9e7b901b39a4f732c))
* **Banner::Sound:** Fix crash when setting a sound for the banner. ([d8fcf3c](https://github.com/Belphemur/SoundSwitch/commit/d8fcf3c604bac5c2c3d87a667aacb03deb226856)), closes [#730](https://github.com/Belphemur/SoundSwitch/issues/730)
* **Device::Volume:** Only get volume when device is active ([40db2ef](https://github.com/Belphemur/SoundSwitch/commit/40db2ef44ffe0b667a666119ec1bd1c7f80e86d3))
* **Profile::Hotkey:** Fix the cycling logic between profiles ([009475b](https://github.com/Belphemur/SoundSwitch/commit/009475bbc3e1f2c3e4b56ba27816d91511d73723)), closes [#734](https://github.com/Belphemur/SoundSwitch/issues/734)
* **Telemetry:** Start telemetry properly ([06ab90c](https://github.com/Belphemur/SoundSwitch/commit/06ab90cadc5cfdc7df10ef16564bad57c02b0188))


### Enhancements

* **Systray::Tooltip:** Add volume % when hovering the SoundSwitch icon. ([9b1aba3](https://github.com/Belphemur/SoundSwitch/commit/9b1aba3e8ea30b971506196d179e95edbe0e8d45))
* **Telemetry:** Add proper tooltip for explaining telemetry ([f90ca7f](https://github.com/Belphemur/SoundSwitch/commit/f90ca7f7a84083a59692e1eeff25f5e2a51484b8)), closes [#736](https://github.com/Belphemur/SoundSwitch/issues/736)
* **Telemetry:** Clarify the term related to telemetry ([8b2e688](https://github.com/Belphemur/SoundSwitch/commit/8b2e68855bbc5e1ecd95764ed3e01187948d103b))


### Languages

* **Hebrew:** Translated Settings using Weblate ([1db0a11](https://github.com/Belphemur/SoundSwitch/commit/1db0a11ad4cb8ab1f7889d89c4f88f78b69bccf6))
* **Hebrew:** Translated Settings using Weblate ([cf7e3eb](https://github.com/Belphemur/SoundSwitch/commit/cf7e3eb9a31d169da436d087051eb483d67cb40c))
* **Hebrew:** Translated Update Download using Weblate ([fa90c65](https://github.com/Belphemur/SoundSwitch/commit/fa90c65e7f3e87f184de940ed04c756738eacacb))
* **Italian:** Translated Settings using Weblate ([1e5871e](https://github.com/Belphemur/SoundSwitch/commit/1e5871e3031a9985e0472d566d26228e25337464))
* **Norwegian Bokmål:** Translated Settings using Weblate ([04675a9](https://github.com/Belphemur/SoundSwitch/commit/04675a92a6fc1afbef45c278dbe30ab2ceb80d4f))
* **Portuguese:** Translated Settings using Weblate ([8c4f733](https://github.com/Belphemur/SoundSwitch/commit/8c4f733411be4a6d1a9c0ab6dddff88f68ab0369))
* **Portuguese:** Translated Settings using Weblate ([872b055](https://github.com/Belphemur/SoundSwitch/commit/872b0556bf2552c76dbd12975cc78ee1d49d03ea))
* **Portuguese:** Translated Update Download using Weblate ([7cb495e](https://github.com/Belphemur/SoundSwitch/commit/7cb495e5e0a909be653adec79688123a02ec4306))
* **Portuguese (Brazil):** Translated Settings using Weblate ([4a35ef5](https://github.com/Belphemur/SoundSwitch/commit/4a35ef59212278e8f64c1e1be7d46c80d365045f))
* **Portuguese (Brazil):** Translated Settings using Weblate ([e0712a4](https://github.com/Belphemur/SoundSwitch/commit/e0712a4c06af7d30a881059c30ccaf4b7094d6f7))
* **Portuguese (Brazil):** Translated Update Download using Weblate ([2c7f3bd](https://github.com/Belphemur/SoundSwitch/commit/2c7f3bd14ddd5dbfdbf11f09087856c2d18b8f5c))
* **Profile::Hotkey:** Update description of hotkey feature ([54e52df](https://github.com/Belphemur/SoundSwitch/commit/54e52dff33277aa43cc287b918b6d27d07fde25e))
* **telemetry:** Add description ([c0a5bdd](https://github.com/Belphemur/SoundSwitch/commit/c0a5bdd8a197b1d062c780df0dd2383ebbaa6251)), closes [#736](https://github.com/Belphemur/SoundSwitch/issues/736)

## [6.0.0](https://github.com/Belphemur/SoundSwitch/compare/v5.11.2...v6.0.0) (2021-08-27)


### ⚠ BREAKING CHANGES

* **QuickMenu:** Quick menu will appear when using hotkey akin to the Windows language menu.

Quick Menu is a new feature that changes the way you can interact with your selected devices. You can disable it in the Settings Menu.

### Features

* **DeviceMenu:** Auto-hide after inactivity to not stay on user screen ([59922d6](https://github.com/Belphemur/SoundSwitch/commit/59922d68652d60714a6355f87cd1a4e066c930c3))
* **Notification::DeviceMenu:** First version of the new device menu triggered by device changed. ([ee6f2c5](https://github.com/Belphemur/SoundSwitch/commit/ee6f2c55ee614fe88cfc78a8140869afed8199f1))
* **QuickMenu:** Display a quick menu on cursor position when the user use a HotKey. ([8d83ad6](https://github.com/Belphemur/SoundSwitch/commit/8d83ad69334879c2995f4d4222dd183e678fd41a))
* **QuickMenu:** The user can enable or disable the quick menu in the settings. ([fd44ca3](https://github.com/Belphemur/SoundSwitch/commit/fd44ca342cfe5fbd8a91bd857e13f42ebb566035)), closes [#625](https://github.com/Belphemur/SoundSwitch/issues/625)
* **Telemetry:** Add setting for telemetry ([38cb95c](https://github.com/Belphemur/SoundSwitch/commit/38cb95ce8b7578874eef18924b2fb4f879624b55))


### Bug Fixes

* **Device::Switching:** Fix not finding the device when the ID has changed ([5741cfd](https://github.com/Belphemur/SoundSwitch/commit/5741cfdbfbf2f216b0129efe26fe81465f03c332))
* **QuickMenu:** Interrupt hiding the quick menu when it's called again ([2ad9500](https://github.com/Belphemur/SoundSwitch/commit/2ad9500708bd5bad8d989fcf1ad7bda99323e2ac))


### Languages

* **Chinese (Simplified):** Translated About using Weblate ([bef1eaa](https://github.com/Belphemur/SoundSwitch/commit/bef1eaa9f94c4fdef86a41a351f7d4ab0f44b983))
* **Dutch:** Translated Settings using Weblate ([7ad3831](https://github.com/Belphemur/SoundSwitch/commit/7ad383156349fec75830f92cda1f445e5451c533))
* **Dutch:** Translated Tray Icon using Weblate ([41f275b](https://github.com/Belphemur/SoundSwitch/commit/41f275b652992255c6a713a7aed0fff1938ed61f))
* **Finnish:** Translated About using Weblate ([e405f00](https://github.com/Belphemur/SoundSwitch/commit/e405f00729112ed2073d448c678020c70b7a4088))
* **German:** Translated Tray Icon using Weblate ([cc008bd](https://github.com/Belphemur/SoundSwitch/commit/cc008bd01c89cad65913f7a01745ffbc43dc8a56))
* **Greek:** Translated About using Weblate ([f30bcc7](https://github.com/Belphemur/SoundSwitch/commit/f30bcc74482ec54392f732438764a089bfec14cb))
* **Hebrew:** Translated Settings using Weblate ([a00e0be](https://github.com/Belphemur/SoundSwitch/commit/a00e0be3754e31c2d597a6e6b941b6bf8f766401))
* **Hebrew:** Translated Settings using Weblate ([209bd49](https://github.com/Belphemur/SoundSwitch/commit/209bd49f6e8be0e7601500cdf7bde79f090eaf9a))
* **Italian:** Translated About using Weblate ([242095c](https://github.com/Belphemur/SoundSwitch/commit/242095c51b5646eddafb531e57555ec33ceaf924))
* **Japanese:** Translated About using Weblate ([babc27e](https://github.com/Belphemur/SoundSwitch/commit/babc27eae3f37d1ef9fdfe1864e981428bd92db7))
* **Korean:** Translated Settings using Weblate ([1aebdb3](https://github.com/Belphemur/SoundSwitch/commit/1aebdb3b1a396cebf893e978cc92f46ca513ec89))
* **Norwegian Bokmål:** Translated About using Weblate ([7be16c3](https://github.com/Belphemur/SoundSwitch/commit/7be16c39a38da9daa57cf6b369448988c31917af))
* **Norwegian Bokmål:** Translated Settings using Weblate ([d0eae02](https://github.com/Belphemur/SoundSwitch/commit/d0eae02befe366d97940e9f6389c40b4d7ab7756))
* **Polish:** Translated About using Weblate ([20f780a](https://github.com/Belphemur/SoundSwitch/commit/20f780a1e4fec11b64f046fceb0ccac93b8ee98b))
* **Portuguese:** Translated Settings using Weblate ([560710f](https://github.com/Belphemur/SoundSwitch/commit/560710f87fcfa246858f1f82979bc3de9514b1e3))
* **Portuguese (Brazil):** Translated Settings using Weblate ([ffc852b](https://github.com/Belphemur/SoundSwitch/commit/ffc852b450702ae6a9cf2e66058a672db5be001f))
* **Portuguese (Brazil):** Translated Settings using Weblate ([39de2ef](https://github.com/Belphemur/SoundSwitch/commit/39de2ef5d918dbbd2999452e0445de52d3c02023))
* **RightToLeft:** Add support for Right to left languages ([f93597c](https://github.com/Belphemur/SoundSwitch/commit/f93597ca0b8d1b06d713393e95e79de39b31242e)), closes [#601](https://github.com/Belphemur/SoundSwitch/issues/601)
* **Russian:** Translated About using Weblate ([bf836a4](https://github.com/Belphemur/SoundSwitch/commit/bf836a443626da6b87d8462c42068434224cb366))
* **Swedish:** Translated About using Weblate ([dc352c4](https://github.com/Belphemur/SoundSwitch/commit/dc352c49074dc1ac7cc4e8193120bfa66cfa0a5e))
* **Swedish:** Translated About using Weblate ([a187b91](https://github.com/Belphemur/SoundSwitch/commit/a187b9196b1932f5af3f73d7f71fa5074c14452e))
* **telemetry:** add localization for telemetry ([227a32c](https://github.com/Belphemur/SoundSwitch/commit/227a32cad951126d6c9d4b8aaeefe80d64dd1b73))

### [5.11.2](https://github.com/Belphemur/SoundSwitch/compare/v5.11.1...v5.11.2) (2021-07-27)


### Bug Fixes

* **Device::Matching:** Always match devices by their Id and their clean name. Also follow user order for display/switching. ([646f126](https://github.com/Belphemur/SoundSwitch/commit/646f12601a0e25d6677dcff309348c66f9ff3671)), closes [#706](https://github.com/Belphemur/SoundSwitch/issues/706)
* **Device::Matching:** Improve the hashcode to clash with other ([718b951](https://github.com/Belphemur/SoundSwitch/commit/718b951634f25d21f3f8ed9f31dc8571ea24ea77))
* **Log:** Remove unneeded warning in the log ([4188c85](https://github.com/Belphemur/SoundSwitch/commit/4188c85560fa50f3ce617ee6cad6f1bbef3d6aea))
* **Update:** Use backoff strategy to avoid sending too much unneeded requests. ([a4c97ab](https://github.com/Belphemur/SoundSwitch/commit/a4c97ab592784515e0c89abb2a0f75b9fa8d89b9))


### Languages

* **Italian:** Translated Tray Icon using Weblate ([fe68a2a](https://github.com/Belphemur/SoundSwitch/commit/fe68a2a724a0c2c10dfdea73464da05cf8c9378f))
* **Italian:** Translated Update Download using Weblate ([85890bc](https://github.com/Belphemur/SoundSwitch/commit/85890bcdf9589484567826640626d6e662c104b5))

### [5.11.1](https://github.com/Belphemur/SoundSwitch/compare/v5.11.0...v5.11.1) (2021-07-25)


### Bug Fixes

* **Device::Switching:** Fix device switching not working when id is different. ([44ba705](https://github.com/Belphemur/SoundSwitch/commit/44ba705e03762b40d4ab111ba65d4d2a2db13841)), closes [#701](https://github.com/Belphemur/SoundSwitch/issues/701)
* **Update:** Make the signature checker less restrictive ([19ccadd](https://github.com/Belphemur/SoundSwitch/commit/19ccadddded39d268355cf38c1584f2bae084f72))

## [5.11.0](https://github.com/Belphemur/SoundSwitch/compare/v5.10.2...v5.11.0) (2021-07-25)


### Features

* **Device::Matching:** Use the Name to match device when id is different. Please rename your device if you have 2 with the same name. ([0dd1ef2](https://github.com/Belphemur/SoundSwitch/commit/0dd1ef261d8e879b778714d2533b2f32d38b47b8))


### Bug Fixes

* **Device::Switching:** Fix case where the app doesn't remember the HDMI device after graphic update ([be97b1b](https://github.com/Belphemur/SoundSwitch/commit/be97b1b7af816a3569ee5e5d0063f181bca3bccd)), closes [#698](https://github.com/Belphemur/SoundSwitch/issues/698)
* **Device::Switching:** Fix device switching not finding all the different selected devices ([9bedd8d](https://github.com/Belphemur/SoundSwitch/commit/9bedd8d918f4bdbb9e0c3a5ee8e87e01b936047b)), closes [#697](https://github.com/Belphemur/SoundSwitch/issues/697)


### Languages

* **Japanese:** Translated Settings using Weblate ([dd1b7eb](https://github.com/Belphemur/SoundSwitch/commit/dd1b7ebf64f44d597470ba585a13f54f0beeb48e))
* **Japanese:** Translated Settings using Weblate ([66c6ab1](https://github.com/Belphemur/SoundSwitch/commit/66c6ab144d41f8f683667b6e85c40a02314c1ffb))
* **Japanese:** Translated Tray Icon using Weblate ([0905aff](https://github.com/Belphemur/SoundSwitch/commit/0905aff78590134f60f2b78563fed1fa2bf0dc16))
* **Japanese:** Translated Update Download using Weblate ([17ac3bf](https://github.com/Belphemur/SoundSwitch/commit/17ac3bf88c7f3144307a91ada6f85a4853054408))

### [5.10.2](https://github.com/Belphemur/SoundSwitch/compare/v5.10.1...v5.10.2) (2021-06-30)


### Bug Fixes

* **Updater:** User agent missing to check for update ([310d948](https://github.com/Belphemur/SoundSwitch/commit/310d948c6a186f92cbc60d494c7dc560c4b15708))
* **Updater::Download:** Fix missing user agent for downloading release ([f039dd4](https://github.com/Belphemur/SoundSwitch/commit/f039dd4b453b5d46565cf33bba67039d6ba8fc95))

### [5.10.1](https://github.com/Belphemur/SoundSwitch/compare/v5.10.0...v5.10.1) (2021-06-30)


### Bug Fixes

* **Notification::Custom:** Fix issue where the custom sound wasn't played properly. ([4e23556](https://github.com/Belphemur/SoundSwitch/commit/4e235560dba18a2093822e8f7ca5b7efdcfbb6ad)), closes [#662](https://github.com/Belphemur/SoundSwitch/issues/662)
* **Notification::Sound:** Fix issue where sound wasn't played properly. ([08ca605](https://github.com/Belphemur/SoundSwitch/commit/08ca605ec44e99f2d8f4f2c67a64cef047dcdb07))
* **Update::Later:** Don't force the user to update when left clicking on the icon ([56199da](https://github.com/Belphemur/SoundSwitch/commit/56199da2ee3f190388f4d60fd07169f8b727ff92))
* **Updater:** Use the right mode to open the installer file for update. ([dd5399f](https://github.com/Belphemur/SoundSwitch/commit/dd5399fdde5a733bec58f044b48b8f39175cc9ee))
* **Updater::Postpone:** Clicking on the menu shouldn't force downloading the postponed release. ([352765b](https://github.com/Belphemur/SoundSwitch/commit/352765b8e57063a6092197258d5e863e862f906f))


### Languages

* **Croatian:** Translated Update Download using Weblate ([9441f44](https://github.com/Belphemur/SoundSwitch/commit/9441f44df9f8f4f7ed80ef7102a773c267fd973e))
* **Danish:** Added Settings translation using Weblate ([3915858](https://github.com/Belphemur/SoundSwitch/commit/3915858b91dff13e566e957dd41bff6e7efb7f21))
* **Hebrew:** Translated Tray Icon using Weblate ([bc35bc4](https://github.com/Belphemur/SoundSwitch/commit/bc35bc4be99ac19f1db1684ad4bd0a1e21c566d8))
* **Hebrew:** Translated Update Download using Weblate ([7272781](https://github.com/Belphemur/SoundSwitch/commit/72727816c61b87a2886f51118584b59520a5332a))
* **Korean:** Translated About using Weblate ([3add52d](https://github.com/Belphemur/SoundSwitch/commit/3add52da4bb2d40734d208e03e88f203ae0001c8))
* **Korean:** Translated Settings using Weblate ([35cd467](https://github.com/Belphemur/SoundSwitch/commit/35cd46780560cb1de1e8d4506ec25a3df846e9b1))
* **Korean:** Translated Tray Icon using Weblate ([648c162](https://github.com/Belphemur/SoundSwitch/commit/648c162011a2fedc3f9b3c8813e64c23a6bec0a4))
* **Korean:** Translated Update Download using Weblate ([e7054f6](https://github.com/Belphemur/SoundSwitch/commit/e7054f6c73987418fbc240a25dafdc6cad8dc68f))
* **Russian:** Translated Settings using Weblate ([ec9884c](https://github.com/Belphemur/SoundSwitch/commit/ec9884ce5dee8084164f832c6e08523a45a6fd33))
* **Russian:** Translated Tray Icon using Weblate ([d214918](https://github.com/Belphemur/SoundSwitch/commit/d214918333991e1854a77f38de2d117301d43344))
* **Russian:** Translated Update Download using Weblate ([5d36dfa](https://github.com/Belphemur/SoundSwitch/commit/5d36dfa6feab263dd9d59e63602276d0c74904e5))
* **Turkish:** Translated Settings using Weblate ([d3f8564](https://github.com/Belphemur/SoundSwitch/commit/d3f8564d3bac89a9939d7ae4648ecf5c6836c4b3))
* **Turkish:** Translated Tray Icon using Weblate ([55ba8f8](https://github.com/Belphemur/SoundSwitch/commit/55ba8f8e2432522991cb32d588c84910556be909))


### Enhancements

* **Device::Switching:** Be sure the order of switching device follow the selection made by the user in the settings menu ([2956bc6](https://github.com/Belphemur/SoundSwitch/commit/2956bc66b3ba53e45f8b932278655ef3b09cb753))

## [5.10.0](https://github.com/Belphemur/SoundSwitch/compare/v5.9.4...v5.10.0) (2021-05-19)


### Features

* **Update:** Let's the user be a to manually check for update by clicking the update menu item. ([8dd63c1](https://github.com/Belphemur/SoundSwitch/commit/8dd63c119ee7b4c30991f3f399a3f1d072b9520b)), closes [#642](https://github.com/Belphemur/SoundSwitch/issues/642)
* **Update::Postpone:** User can now decide to postpone the update and be reminded about it at a later date. ([a2b507e](https://github.com/Belphemur/SoundSwitch/commit/a2b507e7f215deefadda41ca848aec9e000fd239))


### Bug Fixes

* **AutoUpdate::Downloader:** Fix crash when closing the download notify. ([58044d1](https://github.com/Belphemur/SoundSwitch/commit/58044d15b9efcb1003d16e8b67a094516d6f6362))
* **Foreground:** Fix issue where the endpoint would change because of the way foreground feature was disabled. ([0c427fc](https://github.com/Belphemur/SoundSwitch/commit/0c427fc6e65505eca7207ba87ffa24b363b530d5)), closes [#649](https://github.com/Belphemur/SoundSwitch/issues/649)
* **Update:** Rework the way update are check to trigger a check when the setting is changed. ([dfd2f55](https://github.com/Belphemur/SoundSwitch/commit/dfd2f5517406d157f24d46f0e1e4f45df0502db1)), closes [#641](https://github.com/Belphemur/SoundSwitch/issues/641)


### Languages

* **Arabic:** Add arabic to the application. Only in beta. ([3809502](https://github.com/Belphemur/SoundSwitch/commit/38095025b6fd3e8ecf6b166de8d97344f062eae9))
* **Arabic:** Added About translation using Weblate ([21b81bb](https://github.com/Belphemur/SoundSwitch/commit/21b81bb55cf429933bd1785f6c8d213d25d8b102))
* **Arabic:** Added Settings translation using Weblate ([a5ec504](https://github.com/Belphemur/SoundSwitch/commit/a5ec50483cd64def2ee548f65457115427c7c2e4))
* **Arabic:** Added Tray Icon translation using Weblate ([b7ec4c3](https://github.com/Belphemur/SoundSwitch/commit/b7ec4c3bd6f54d86f689695acacea5e5c03a2c5a))
* **Arabic:** Added Update Download translation using Weblate ([5bc1aa1](https://github.com/Belphemur/SoundSwitch/commit/5bc1aa142b338f5267e3c54c08baf99fd4276788))
* **Arabic:** Translated About using Weblate ([1416129](https://github.com/Belphemur/SoundSwitch/commit/14161296d9153b0d1dede0692f0582ac5b044766))
* **Arabic:** Translated About using Weblate ([be0c28a](https://github.com/Belphemur/SoundSwitch/commit/be0c28a3d11337fe14eb4d2a768e8377bdda5d91))
* **Arabic:** Translated Settings using Weblate ([957eca5](https://github.com/Belphemur/SoundSwitch/commit/957eca56bc58407d74e276e8553f930e354f3cf8))
* **Arabic:** Translated Settings using Weblate ([6b27b68](https://github.com/Belphemur/SoundSwitch/commit/6b27b681d26e29aa4ac6b816b4f07135226031b1))
* **Arabic:** Translated Tray Icon using Weblate ([283ba8f](https://github.com/Belphemur/SoundSwitch/commit/283ba8f4a405429b66c1fb9ab598a5ce4988cdf1))
* **Arabic:** Translated Tray Icon using Weblate ([3e8f7cc](https://github.com/Belphemur/SoundSwitch/commit/3e8f7ccc3f5cb712caf0d467b0978a568131ebc3))
* **Arabic:** Translated Update Download using Weblate ([bf220df](https://github.com/Belphemur/SoundSwitch/commit/bf220dfc6c2e3a6e4b6082cde8ad4398e6be2d68))
* **Chinese (Simplified):** Translated Tray Icon using Weblate ([4bda8eb](https://github.com/Belphemur/SoundSwitch/commit/4bda8eb989d3b5d024200516fab6bd05eb97a55a))
* **Chinese (Simplified):** Translated Update Download using Weblate ([452ec1c](https://github.com/Belphemur/SoundSwitch/commit/452ec1c9753ea7863b4d4373cd485c05bee63eef))
* **Croatian:** Translated Tray Icon using Weblate ([44d204f](https://github.com/Belphemur/SoundSwitch/commit/44d204f9fd0821d4e710ae84bacd5ebb5a6d010a))
* **Italian:** Translated Settings using Weblate ([69cd836](https://github.com/Belphemur/SoundSwitch/commit/69cd836ac998474037f90043815462620924810e))
* **Norwegian Bokmål:** Translated Tray Icon using Weblate ([533b0b1](https://github.com/Belphemur/SoundSwitch/commit/533b0b1aa1ce29b532d527db3b1625d6eff625dc))
* **Portuguese:** Translated Settings using Weblate ([917c103](https://github.com/Belphemur/SoundSwitch/commit/917c103e6255f40daf2a8c284c0818a2cba0252b))
* **Portuguese:** Translated Tray Icon using Weblate ([be6ab26](https://github.com/Belphemur/SoundSwitch/commit/be6ab2652ed7d133177038a3329a498aa1ef75c4))
* **Portuguese:** Translated Update Download using Weblate ([f736183](https://github.com/Belphemur/SoundSwitch/commit/f736183665671715c908fa6f00cd7d2012f04968))
* **Portuguese (Brazil):** Translated Tray Icon using Weblate ([e82777b](https://github.com/Belphemur/SoundSwitch/commit/e82777b6b341d9ce6088c488eece2689ad08771c))
* **Portuguese (Brazil):** Translated Update Download using Weblate ([55121e4](https://github.com/Belphemur/SoundSwitch/commit/55121e4d5f9297909c3d6c5ba903ba7ab52796a1))


### Enhancements

* **Help:** Help menu send to the discussion on GitHub for the user to be able to ask his questions. ([e9fde1c](https://github.com/Belphemur/SoundSwitch/commit/e9fde1ca7e4d9133c57893f092ecd966e65a9dd2))
* **Settings:** Increase default size of Setting menu to work better with different language. ([d007d80](https://github.com/Belphemur/SoundSwitch/commit/d007d809d7a00d1ce6967f261822c918813801de))
* **Update:** Clicking on the update menu item will trigger update ([34a1131](https://github.com/Belphemur/SoundSwitch/commit/34a1131b4df812c8a813d743fd16fc4a2e0316f4)), closes [#641](https://github.com/Belphemur/SoundSwitch/issues/641)
* **Update:** Don't autodownload when opening the update form. Only after the user click install. ([2029a9b](https://github.com/Belphemur/SoundSwitch/commit/2029a9b44641341521d5e67197040c03a8a61c00)), closes [#528](https://github.com/Belphemur/SoundSwitch/issues/528)
* **Update:** Improve the way SoundSwitch check for new updates. ([8823e7a](https://github.com/Belphemur/SoundSwitch/commit/8823e7acf9592b573af8c6f042c93ee3c92bc3e7))
* **Update::Form:** Make the title of the download window be the name of the release. ([1da4f2c](https://github.com/Belphemur/SoundSwitch/commit/1da4f2c0675df4aa5dd713f7eed42a302dfb52a5))
* **Update::Postpone:** The more the user postpone, the longer it waits before asking to update. ([355dd69](https://github.com/Belphemur/SoundSwitch/commit/355dd69216db91293c361419e95ce9b95a1a789a)), closes [#528](https://github.com/Belphemur/SoundSwitch/issues/528)

### [5.9.4](https://github.com/Belphemur/SoundSwitch/compare/v5.9.3...v5.9.4) (2021-05-11)


### Bug Fixes

* **Banner::Sound:** Catch execption that could be thrown. ([c7a3059](https://github.com/Belphemur/SoundSwitch/commit/c7a3059c082eadbc6a6f1f0e8c14f3566b83bc37))
* **Configuration:** Fix likely corruption of configuration. ([60cef81](https://github.com/Belphemur/SoundSwitch/commit/60cef8184720b0d7843b38ab2ebff056b0a848d7))
* **Program:** Crash when stopping the application ([8da165d](https://github.com/Belphemur/SoundSwitch/commit/8da165d72721b8b473656fde141359ba4efce631))
* **TrayIcon::Menu:** Fix issue with the menu not showing up when it should. ([8205809](https://github.com/Belphemur/SoundSwitch/commit/820580944a277334fc8ddf53732bbf3cfa2d64cf)), closes [#635](https://github.com/Belphemur/SoundSwitch/issues/635)
* **TrayIcon::Tooltip:** Fix issue where the trayIcon tooltip kept being rebuilt. ([ca188bd](https://github.com/Belphemur/SoundSwitch/commit/ca188bdff5ca89b78ef7dc3d1fcb1be24be47d7e))


### Enhancements

* **Foreground::Switch:** Disable switching foreground application for everybody. Feature should only be on for people that needs it. ([09a8228](https://github.com/Belphemur/SoundSwitch/commit/09a8228459acaea836277bb63c0dd2196559b0e8)), closes [#636](https://github.com/Belphemur/SoundSwitch/issues/636)


### Languages

* **Chinese (Simplified):** Translated Settings using Weblate ([e664f6c](https://github.com/Belphemur/SoundSwitch/commit/e664f6c7637e6c5600ab578be1d05816c80216c3))

### [5.9.3](https://github.com/Belphemur/SoundSwitch/compare/v5.9.2...v5.9.3) (2021-05-08)


### Bug Fixes

* **Config:** Fix issue when loading and writting to the configuration ([76e60a5](https://github.com/Belphemur/SoundSwitch/commit/76e60a5b93c595368fa9963b070e931611eca345))
* **UrlOpening:** Fix rare case where the URL doesn't open ([aae2ba6](https://github.com/Belphemur/SoundSwitch/commit/aae2ba6fa3ae8b108db2c76bd675bfa34014df96))


### Languages

* **Portugese:** Merge portugese ([b605658](https://github.com/Belphemur/SoundSwitch/commit/b605658c58ee7fd74626bba2b0f0301f69e9972c))
* **Portuguese:** Translated Settings using Weblate ([43ce732](https://github.com/Belphemur/SoundSwitch/commit/43ce73204009ed8ba0fc679af306ac51fdf3810b))
* **Portuguese (Portugal):** Translated Settings using Weblate ([b558c0d](https://github.com/Belphemur/SoundSwitch/commit/b558c0d789c0d6591de0ca3e8ddd03f74c82fe57))
* **Turkish:** Translated About using Weblate ([fa83b8e](https://github.com/Belphemur/SoundSwitch/commit/fa83b8e74eee4287df9ebd992d9f02998539e156))
* **Turkish:** Translated Settings using Weblate ([2e5da36](https://github.com/Belphemur/SoundSwitch/commit/2e5da361ea837aa5194422319c01bc54fc8f7201))
* **Turkish:** Translated Tray Icon using Weblate ([b788f1f](https://github.com/Belphemur/SoundSwitch/commit/b788f1f0fdaae0b34d0722137e7157359395fbf9))

### [5.9.2](https://github.com/Belphemur/SoundSwitch/compare/v5.9.1...v5.9.2) (2021-05-08)


### Bug Fixes

* **Foreground:** Fix unhandled crash in foreground window detection ([b61c347](https://github.com/Belphemur/SoundSwitch/commit/b61c3475c5dbb584be5603c51487891e894527ab))
* **HotKey::Recording:** Disable hotkey for recording if can't register it. ([0a5da99](https://github.com/Belphemur/SoundSwitch/commit/0a5da99b1a59ce8967c67bda94ece977cdeabc59))
* **Microphone::Mute:** Stop telling user that the microphone mute hotkey couldn't be registered ([30d021f](https://github.com/Belphemur/SoundSwitch/commit/30d021fa70887cb9f15dce907f7facf3709c6c13))
* **Profile::TrayIcon:** possible null case when profile manager isn't defined yet ([234d5bf](https://github.com/Belphemur/SoundSwitch/commit/234d5bf07e9d77e8c774d6ce2be5ae9330573acf))

### [5.9.1](https://github.com/Belphemur/SoundSwitch/compare/v5.9.0...v5.9.1) (2021-05-08)


### Bug Fixes

* **DeviceLister:** Fix concurrency issue with the TrayIcon ([84ddc78](https://github.com/Belphemur/SoundSwitch/commit/84ddc788db04fea3e073a5dbc4b9a6dedb37f658)), closes [#626](https://github.com/Belphemur/SoundSwitch/issues/626) [#622](https://github.com/Belphemur/SoundSwitch/issues/622)
* **DeviceLister:** Fix concurrency issue with the TrayIcon ([a73a0de](https://github.com/Belphemur/SoundSwitch/commit/a73a0de7a1921995e7131952f35e433e7b48cac3)), closes [#626](https://github.com/Belphemur/SoundSwitch/issues/626) [#625](https://github.com/Belphemur/SoundSwitch/issues/625)


### Languages

* **Hebrew:** Translated Settings using Weblate ([96225dd](https://github.com/Belphemur/SoundSwitch/commit/96225dd5d9ca128efeafa6db60147529d5dd15d6))
* **Hebrew:** Translated Update Download using Weblate ([a8acb4f](https://github.com/Belphemur/SoundSwitch/commit/a8acb4f3060973808508498def1cadbb005ec5c1))
* **Portuguese:** Translated About using Weblate ([c58c3c7](https://github.com/Belphemur/SoundSwitch/commit/c58c3c7dea374eea345f326dd0ae0d51a70a4cf2))
* **Portuguese:** Translated Settings using Weblate ([e29ab0d](https://github.com/Belphemur/SoundSwitch/commit/e29ab0d6880e471efbbcaf9ba1761381f6b277c6))
* **Portuguese:** Translated Tray Icon using Weblate ([908c67d](https://github.com/Belphemur/SoundSwitch/commit/908c67d35e415d191aace4f35da788edd3f07c34))
* **Portuguese:** Translated Update Download using Weblate ([b0c9ddc](https://github.com/Belphemur/SoundSwitch/commit/b0c9ddc8c2b2b33010d193365b349514a836c71f))
* **Spanish:** Translated About using Weblate ([d66c9b8](https://github.com/Belphemur/SoundSwitch/commit/d66c9b8a5303353cbf5bc2b57ad32f1f92d940db))
* **Spanish:** Translated Settings using Weblate ([8eb75b3](https://github.com/Belphemur/SoundSwitch/commit/8eb75b37dce8f56f2666f7fdbfb6f9f7f2c880a3))
* **Spanish:** Translated Tray Icon using Weblate ([a97ff96](https://github.com/Belphemur/SoundSwitch/commit/a97ff96a40ebbb2721fdf22011f4e539db220799))
* **Spanish:** Translated Update Download using Weblate ([1bf75ca](https://github.com/Belphemur/SoundSwitch/commit/1bf75ca5b50c65d8bebe7a4e357ac80173695c4d))
* **Turkish:** Translated Settings using Weblate ([e789a62](https://github.com/Belphemur/SoundSwitch/commit/e789a6280caa32b4b2a9407a48fdde8cb282290c))
* **Turkish:** Translated Tray Icon using Weblate ([19e2829](https://github.com/Belphemur/SoundSwitch/commit/19e28296a61faf5e5345c14c2863701c40bd6112))
* **Turkish:** Translated Update Download using Weblate ([5e4921c](https://github.com/Belphemur/SoundSwitch/commit/5e4921cab0d2a75c432545c52c3383fe4aeb5b6e))


### Enhancements

* **ErrorReporting:** Add proper error reporting ([44efc90](https://github.com/Belphemur/SoundSwitch/commit/44efc90ef1c047b83df8f4a8d3571ebf4c4fa7b3)), closes [#546](https://github.com/Belphemur/SoundSwitch/issues/546)
* **Profile::TrayIcon:** Update the menu to have the profile directly in the menu ([54003ea](https://github.com/Belphemur/SoundSwitch/commit/54003ea46355885f0977257535aa68828e7cecc6)), closes [#628](https://github.com/Belphemur/SoundSwitch/issues/628)

## [5.9.0](https://github.com/Belphemur/SoundSwitch/compare/v5.8.3...v5.9.0) (2021-05-01)


### Features

* **Profile::Trigger::TrayIcon:** Add tray icon as trigger ([4a43fa5](https://github.com/Belphemur/SoundSwitch/commit/4a43fa5d66406dfb6120c57ef4ef3728885a43b0)), closes [#492](https://github.com/Belphemur/SoundSwitch/issues/492)


### Bug Fixes

* **Device:** Listing device causing application hanging ([55d7316](https://github.com/Belphemur/SoundSwitch/commit/55d73166af3ec2f3a044c640f97d0220d3952048))
* **Notification::Windows:** Fix crash when using windows notification and mute microphone ([78053fb](https://github.com/Belphemur/SoundSwitch/commit/78053fb0080e8cf57e33b539401552ecdaf77db2)), closes [#596](https://github.com/Belphemur/SoundSwitch/issues/596)
* **Profile::Trigger:** Fix not calling the method in switch ([4b75a25](https://github.com/Belphemur/SoundSwitch/commit/4b75a25f91220cdc8d037cae5b251fb5d2664729))


### Enhancements

* **Profile:** Update the icon of the profile ([4cf5ab2](https://github.com/Belphemur/SoundSwitch/commit/4cf5ab2b31b87af5fbd521687737fb257878fd6a))
* **Profile:** Update the icons for the Profile feature ([37b1f22](https://github.com/Belphemur/SoundSwitch/commit/37b1f2251dc8456f6b052d8fbcdcb8ce09c0e0d2))
* **Profile::Trigger:** Add new TrayMenu trigger to available triggers ([7d4e3d0](https://github.com/Belphemur/SoundSwitch/commit/7d4e3d00f8d2ba7417c6fe1cddd800a71d84af10))


### Languages

* **Croatian:** Translated Settings using Weblate ([3938217](https://github.com/Belphemur/SoundSwitch/commit/393821791e115e88a5fccbcf6f7ae50ae2754186))
* **Hebrew:** Translated Settings using Weblate ([2a6c378](https://github.com/Belphemur/SoundSwitch/commit/2a6c378fafcf31ffedc9d86b977d40102345223c))
* **Hebrew:** Translated Settings using Weblate ([58efe45](https://github.com/Belphemur/SoundSwitch/commit/58efe455e44c27718254ddeac3b66670011443f8))
* **Korean:** Translated About using Weblate ([3aa5a12](https://github.com/Belphemur/SoundSwitch/commit/3aa5a1203f5a7c75d2c04057b3d18337b0829908))
* **Korean:** Translated Settings using Weblate ([5898dcb](https://github.com/Belphemur/SoundSwitch/commit/5898dcb659b812228a06f75a91e873c212ca93d9))
* **Korean:** Translated Settings using Weblate ([a9d70d3](https://github.com/Belphemur/SoundSwitch/commit/a9d70d318513920a460f4b6b282d77698a5ad725))
* **Korean:** Translated Settings using Weblate ([77fe485](https://github.com/Belphemur/SoundSwitch/commit/77fe485c63f3f29d0144fcba107d19b902fbc006))
* **Korean:** Translated Settings using Weblate ([a8e3ec3](https://github.com/Belphemur/SoundSwitch/commit/a8e3ec39a9446185563fa83b3b7f80ab51fbe956))
* **Korean:** Translated Tray Icon using Weblate ([a66510b](https://github.com/Belphemur/SoundSwitch/commit/a66510babd4d588500bc00c55997886288ad3f0c))
* **Portuguese (Brazil):** Translated Settings using Weblate ([e11acfd](https://github.com/Belphemur/SoundSwitch/commit/e11acfd34d7e29bbc8406c6f3179c63e8e7f898d))
* **Turkish:** Add language to the application ([6d3c40c](https://github.com/Belphemur/SoundSwitch/commit/6d3c40c1d008b7d0690c174f1094198d6d6f7267)), closes [#588](https://github.com/Belphemur/SoundSwitch/issues/588)
* **Turkish:** Translated About using Weblate ([d13a888](https://github.com/Belphemur/SoundSwitch/commit/d13a88874aae2245c2878476ee6f77f4400240a6))
* **Turkish:** Translated About using Weblate ([f752680](https://github.com/Belphemur/SoundSwitch/commit/f752680e254a28dec0e14198279849a61887c225))
* **Turkish:** Translated Settings using Weblate ([ff0098d](https://github.com/Belphemur/SoundSwitch/commit/ff0098d77fe6140f59b146df2f3a115a7e36e0a0))
* **Turkish:** Translated Settings using Weblate ([fd4014d](https://github.com/Belphemur/SoundSwitch/commit/fd4014d2d6d75f0fe85c8a64a9df3c9c876de8ae))
* **Turkish:** Translated Settings using Weblate ([ff2685b](https://github.com/Belphemur/SoundSwitch/commit/ff2685b15889f3e662ca67b14777062d31e95766))
* **Turkish:** Translated Tray Icon using Weblate ([1f83dc6](https://github.com/Belphemur/SoundSwitch/commit/1f83dc690ab0b10f5da4b19951c824defebf9ed1))
* **Turkish:** Translated Update Download using Weblate ([f1de471](https://github.com/Belphemur/SoundSwitch/commit/f1de471f754d1fa2aa35652d6df9bf8d6a10f380))
* **Turkish:** Translated Update Download using Weblate ([9ce1bfe](https://github.com/Belphemur/SoundSwitch/commit/9ce1bfe56633e487ac041a26a42d89b5b38a736f))

### [5.8.3](https://github.com/Belphemur/SoundSwitch/compare/v5.8.2...v5.8.3) (2021-04-11)


### Bug Fixes

* **Czech:** Fix recognition of the Czech language ([c4faedc](https://github.com/Belphemur/SoundSwitch/commit/c4faedcd2b6f926c6221b9803bca12ffba6b2b9b)), closes [#586](https://github.com/Belphemur/SoundSwitch/issues/586) [#590](https://github.com/Belphemur/SoundSwitch/issues/590)
* **language:** Use the native name of the language ([7f3b18d](https://github.com/Belphemur/SoundSwitch/commit/7f3b18d808a307ddbbb4576987975ee116ddc11b))
* **UI::Profile:** Add proper sorting ([7a8e8fb](https://github.com/Belphemur/SoundSwitch/commit/7a8e8fb0bc76b0654e02c536bf5b52968d89d3dc)), closes [#589](https://github.com/Belphemur/SoundSwitch/issues/589)
* **UI::Profile:** Possible issue with profile UI ([e795ef1](https://github.com/Belphemur/SoundSwitch/commit/e795ef1e377a66e5cb6372ad96b73cc2eaf2b64c))


### Languages

* **Hungarian:** Translated Settings using Weblate ([b206a75](https://github.com/Belphemur/SoundSwitch/commit/b206a754854c2df3509be2e9bb59413bc5368704))
* **Portuguese (Brazil):** Translated About using Weblate ([1d20c9f](https://github.com/Belphemur/SoundSwitch/commit/1d20c9fbf62ab17da19bdc7cd7d64b80e1fddbb4))
* **Portuguese (Brazil):** Translated Settings using Weblate ([94054bd](https://github.com/Belphemur/SoundSwitch/commit/94054bde5e03ec51c08a11dbcecb89b0881d3bd6))
* **Turkish:** Add UpdateDownload ([dc5d85d](https://github.com/Belphemur/SoundSwitch/commit/dc5d85d3a329e0adea3dfe3c625854d3d4741c44))
* **Turkish:** Added About translation using Weblate ([ae15091](https://github.com/Belphemur/SoundSwitch/commit/ae15091a678b2f27de1bff1a31caea3439bea971))
* **Turkish:** Added Settings translation using Weblate ([69c939a](https://github.com/Belphemur/SoundSwitch/commit/69c939a9624528b122ebd75fca31d5f6ecf9ab3e))
* **Turkish:** Added Tray Icon translation using Weblate ([1cdb817](https://github.com/Belphemur/SoundSwitch/commit/1cdb81761dba046bf569b1ae8e3a2e6242ac259d))


### Tests

* **Language:** Add test to check the language ([2a08afe](https://github.com/Belphemur/SoundSwitch/commit/2a08afe62025e20c82c4aca5547b46d69884820f))

### [5.8.2](https://github.com/Belphemur/SoundSwitch/compare/v5.8.1...v5.8.2) (2021-04-11)


### Bug Fixes

* **Banner:** Problem with sound not playing correctly ([ce90164](https://github.com/Belphemur/SoundSwitch/commit/ce901649b0061a458202436946efde87674da469))
* **Mute:** Possible issue with using wrong instance of object ([c3818cf](https://github.com/Belphemur/SoundSwitch/commit/c3818cf3828bd7da9978f2b87e4161df9f9e8a1b))
* **UI:** Display issue with the profile tab ([889bf40](https://github.com/Belphemur/SoundSwitch/commit/889bf406829cee3999ea82c33600a3514e8a92cb))
* **UI:** Missing delete icon for sound on banner notif ([e8dc98a](https://github.com/Belphemur/SoundSwitch/commit/e8dc98aa0c6251798e215ea7c461eb0fd2f9503c))


### Languages

* **Chinese (Simplified):** Translated Settings using Weblate ([1373827](https://github.com/Belphemur/SoundSwitch/commit/137382729361a357fb0ec25e8a01d0ca7c4dbe26))
* **Croatian:** Translated Settings using Weblate ([760dac8](https://github.com/Belphemur/SoundSwitch/commit/760dac8d8624cd48f51cb56ff21b3b32028077da))
* **Croatian:** Translated Settings using Weblate ([adce437](https://github.com/Belphemur/SoundSwitch/commit/adce4375a7b92cd0f815e4ea9cf2c351d16b52dc))
* **Czech:** Add Czech language ([2034aa6](https://github.com/Belphemur/SoundSwitch/commit/2034aa62ef9c380dcc71520b11919a429bce8c2e))
* **Czech:** Translated using Weblate ([4f31a8b](https://github.com/Belphemur/SoundSwitch/commit/4f31a8b79cbf257cb47c0c963c4134728726688a))
* **Czech:** Translated using Weblate ([bff272e](https://github.com/Belphemur/SoundSwitch/commit/bff272e7adca85c8b582157ae5d4b9162190505b))
* **Czech:** Translated using Weblate ([1708699](https://github.com/Belphemur/SoundSwitch/commit/1708699f127665ff8d9c37f2dad3cd7fdc436869))
* **Czech:** Translated using Weblate ([eb2e1e8](https://github.com/Belphemur/SoundSwitch/commit/eb2e1e858f24e12c963051d35c7b3441a57a9337))
* **German:** Translated Settings using Weblate ([7e607a8](https://github.com/Belphemur/SoundSwitch/commit/7e607a8423393092e40bc3e626063f482aba8227))
* **Hebrew:** Add hebrew language ([2fc5fbc](https://github.com/Belphemur/SoundSwitch/commit/2fc5fbc01b40ca22f52ba30451672bc4f839c043))
* **Hebrew:** Translated using Weblate ([71bc510](https://github.com/Belphemur/SoundSwitch/commit/71bc5103254de6e3f3ff12c28a94d1251593cf62))
* **Hebrew:** Translated using Weblate ([b72ca12](https://github.com/Belphemur/SoundSwitch/commit/b72ca12948b8c9b4c4b2b36d10f9adfa60f3b940))
* **Hebrew:** Translated using Weblate ([b45843a](https://github.com/Belphemur/SoundSwitch/commit/b45843a4374774d03a550472db09b833975386d4))
* **Korean:** Translated using Weblate ([f2c14a4](https://github.com/Belphemur/SoundSwitch/commit/f2c14a45234460da2ecbedf8fadec4fba8735e6f))
* **Russian:** Translated using Weblate ([94f8a2f](https://github.com/Belphemur/SoundSwitch/commit/94f8a2f753f2bef80637ec37517c3ff942406ec4))

## [5.8.1](https://github.com/Belphemur/SoundSwitch/compare/v5.8.0...v5.8.1) (2021-04-03)


### Bug Fixes

* **Settings:** Not being able to set hotkeys ([ac05bcd](https://github.com/Belphemur/SoundSwitch/commit/ac05bcd7c46316ebcba0e784c840d568341c8c28)), closes [#565](https://github.com/Belphemur/SoundSwitch/issues/565)

# [5.8.0](https://github.com/Belphemur/SoundSwitch/compare/v5.7.2...v5.8.0) (2021-04-02)


### Bug Fixes

* **AudioSwitcher:** Add missing return type ([777d5a8](https://github.com/Belphemur/SoundSwitch/commit/777d5a83fb71b2cdd10902ec97c54395ed96420c))
* **Language:** Add Japanese language ([d02a3c3](https://github.com/Belphemur/SoundSwitch/commit/d02a3c3d401502aa7503da0f3819e0e8a63bdf72)), closes [#552](https://github.com/Belphemur/SoundSwitch/issues/552)
* **Language:** CultureInfo for Japanese ([1bd87cd](https://github.com/Belphemur/SoundSwitch/commit/1bd87cd985c16288168106ced67c8a0c031ea237))
* **Mute:** Set default shortcut one not taken by windows ([c3cda41](https://github.com/Belphemur/SoundSwitch/commit/c3cda4122d4a543f68310463752c3b0f575d8946))
* **Mute:** Use the right context to interact with the MMDevice ([4ef87a4](https://github.com/Belphemur/SoundSwitch/commit/4ef87a4e2a1f6cd6797c5dca28b66d2b5ff4acda))
* **updater:** Be sure to check for case where there isn't an installer ([4038b77](https://github.com/Belphemur/SoundSwitch/commit/4038b77c6cf58bfb2e19dc34057ea5319432b5dc))


### Features

* **AudioSwitcher:** Add method to interact directly with a MMDevice in the ComThread ([a8c95d4](https://github.com/Belphemur/SoundSwitch/commit/a8c95d46a43c1f3ea39b9e417e89ebe1d167fe87))
* **Mute:** Add localization and proper spacing ([c062df5](https://github.com/Belphemur/SoundSwitch/commit/c062df5b3f168cfaa6e92e5b4f5c1c3ed710444e))
* **Mute:** Add new string for muted/unmuted ([7334fa4](https://github.com/Belphemur/SoundSwitch/commit/7334fa4876fd775bae1832f150acf05010e70bea))
* **Mute:** Add notification for microphone muted ([6eb124f](https://github.com/Belphemur/SoundSwitch/commit/6eb124f31b417c4fc7beb50d030761ecf5a7b389))
* **Mute:** Add service to mute default microphone ([cbe121c](https://github.com/Belphemur/SoundSwitch/commit/cbe121cb94f31e40ea65c246b636913109843d6f))
* **Mute:** Add the mute feature to UI ([f03f427](https://github.com/Belphemur/SoundSwitch/commit/f03f427a390ef2303566af057d27fa46978395f6))
* **Mute:** Muted state has higher priority ([00941b8](https://github.com/Belphemur/SoundSwitch/commit/00941b8c0fb72de028612353737f57b8686d13dc))
* **Mute:** plug the service to the notification ([bb4baa6](https://github.com/Belphemur/SoundSwitch/commit/bb4baa675b14fd7eb9dfb5b9a304805047194c15))
* **Mute:** Return the state of mute after action ([6d63226](https://github.com/Belphemur/SoundSwitch/commit/6d632264063c581cba93be09e12fb7036226c47e))
* **USB:** Add detection if USB audio device ([13286bd](https://github.com/Belphemur/SoundSwitch/commit/13286bd7e0790f9c808cc04ba2814eddfdefa5c6))


# OLD Changelog

## [v5.7.1](https://github.com/Belphemur/SoundSwitch/tree/v5.7.1) (2021-01-30)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.7.0...v5.7.1)

**Implemented enhancements:**

- Win modifier for shortcut [\#535](https://github.com/Belphemur/SoundSwitch/issues/535)

**Fixed bugs:**

- Webclient exception when SoundSwitch tries to update multiple times [\#543](https://github.com/Belphemur/SoundSwitch/issues/543)
- Windows Notification option displays Banner \(not Windows Notification\) [\#539](https://github.com/Belphemur/SoundSwitch/issues/539)
- Installer shouldn't remember the state of "remove SoundSwitch config" when updating [\#538](https://github.com/Belphemur/SoundSwitch/issues/538)
- New option "正體字/繁體字" is not work correctly [\#537](https://github.com/Belphemur/SoundSwitch/issues/537)
- Updater window open pages inside itself [\#536](https://github.com/Belphemur/SoundSwitch/issues/536)

**Closed issues:**

- Crashing when canceling multiple updates  [\#544](https://github.com/Belphemur/SoundSwitch/issues/544)
- Unable to add single monitor sound source [\#542](https://github.com/Belphemur/SoundSwitch/issues/542)

## [v5.7.0](https://github.com/Belphemur/SoundSwitch/tree/v5.7.0) (2021-01-12)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.6.1...v5.7.0)

**Implemented enhancements:**

- Add translation for Traditional Chinese \(zh\_Hant\) [\#531](https://github.com/Belphemur/SoundSwitch/issues/531)

**Fixed bugs:**

- Taskbar popup not responding and sound lost on headset [\#532](https://github.com/Belphemur/SoundSwitch/issues/532)

**Closed issues:**

- When switching the Systray Icon to Recording mode [\#533](https://github.com/Belphemur/SoundSwitch/issues/533)
- Crash when there is no output device available after PC starts. [\#530](https://github.com/Belphemur/SoundSwitch/issues/530)
- Unable to Switch Speakers [\#529](https://github.com/Belphemur/SoundSwitch/issues/529)

## [v5.6.1](https://github.com/Belphemur/SoundSwitch/tree/v5.6.1) (2020-11-23)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.6.0...v5.6.1)

**Fixed bugs:**

- Can't switch audio device because of "Also switch foreground program" & Profile for applications don't work anymore [\#524](https://github.com/Belphemur/SoundSwitch/issues/524)

## [v5.6.0](https://github.com/Belphemur/SoundSwitch/tree/v5.6.0) (2020-11-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.5.4...v5.6.0)

**Implemented enhancements:**

- Move to use .NET 5.0 [\#523](https://github.com/Belphemur/SoundSwitch/issues/523)
- Move the restore device checkbox to the profile settings to make it more user friendly [\#521](https://github.com/Belphemur/SoundSwitch/issues/521)
- Add a profile trigger for UWP apps [\#518](https://github.com/Belphemur/SoundSwitch/issues/518)
- Add recent version to winget-pkgs? [\#512](https://github.com/Belphemur/SoundSwitch/issues/512)

**Fixed bugs:**

- Restore device choice not saved when editing profile [\#522](https://github.com/Belphemur/SoundSwitch/issues/522)

## [v5.5.4](https://github.com/Belphemur/SoundSwitch/tree/v5.5.4) (2020-11-07)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.5.3...v5.5.4)

**Implemented enhancements:**

- When using a profile with Application and Also Switch default device, let the user choose if he wants to restore the old devices [\#517](https://github.com/Belphemur/SoundSwitch/issues/517)
- \[Enhancement\] Mirror outputs [\#230](https://github.com/Belphemur/SoundSwitch/issues/230)

**Fixed bugs:**

- App crashes when update is declined [\#516](https://github.com/Belphemur/SoundSwitch/issues/516)

**Closed issues:**

- Pop-up shown too often [\#515](https://github.com/Belphemur/SoundSwitch/issues/515)
- Crash on Startup [\#508](https://github.com/Belphemur/SoundSwitch/issues/508)

**Merged pull requests:**

- Adding in setting to always use primary screen for banner notifications. [\#514](https://github.com/Belphemur/SoundSwitch/pull/514) ([westonhowe98](https://github.com/westonhowe98))

## [v5.5.3](https://github.com/Belphemur/SoundSwitch/tree/v5.5.3) (2020-09-12)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.5.2...v5.5.3)

**Implemented enhancements:**

- Is there any chance of supporting changing of the "default format" control panel option?  [\#501](https://github.com/Belphemur/SoundSwitch/issues/501)
- Setting audio levels of currently active device [\#452](https://github.com/Belphemur/SoundSwitch/issues/452)
- Show volume slider like the volume system icon [\#209](https://github.com/Belphemur/SoundSwitch/issues/209)

**Fixed bugs:**

- \[Minor Bug\] Decimals in Sound device names cause truncated display in drop down menus [\#504](https://github.com/Belphemur/SoundSwitch/issues/504)
- Backspace key not working while Soundwitch is running [\#503](https://github.com/Belphemur/SoundSwitch/issues/503)
- Profiles without change default device may wont work [\#500](https://github.com/Belphemur/SoundSwitch/issues/500)
- Show dependency between 'Also switch default device' and 'Communication' in profile dialog [\#498](https://github.com/Belphemur/SoundSwitch/issues/498)
- No scroll bar on Add Profile window when size smaller than the content.  [\#496](https://github.com/Belphemur/SoundSwitch/issues/496)
- Crash on open update details when ussing Classic Windows theme [\#220](https://github.com/Belphemur/SoundSwitch/issues/220)

**Closed issues:**

- Crash after startup on Windows 10 [\#502](https://github.com/Belphemur/SoundSwitch/issues/502)

## [v5.5.2](https://github.com/Belphemur/SoundSwitch/tree/v5.5.2) (2020-08-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.5.1...v5.5.2)

**Fixed bugs:**

- Updating from 5.5.0 to 5.5.1 crashed after installation [\#495](https://github.com/Belphemur/SoundSwitch/issues/495)

**Closed issues:**

- crash report: System.Threading.ThreadStateException: Instance isn't set even after waiting 1250 ms [\#493](https://github.com/Belphemur/SoundSwitch/issues/493)

## [v5.5.1](https://github.com/Belphemur/SoundSwitch/tree/v5.5.1) (2020-08-19)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.5.0...v5.5.1)

**Implemented enhancements:**

- When Steam Big Picture is closed, returned the audio default devices as they were before switching [\#490](https://github.com/Belphemur/SoundSwitch/issues/490)
- Request: Run profile on startup [\#489](https://github.com/Belphemur/SoundSwitch/issues/489)
- Change "No update available" text when update-check is disabled [\#487](https://github.com/Belphemur/SoundSwitch/issues/487)

**Fixed bugs:**

- High DPI Mode not working anymore \(4K screen with zooming 150%\) [\#494](https://github.com/Belphemur/SoundSwitch/issues/494)
- Wrong error message when adding a profile without triggers [\#488](https://github.com/Belphemur/SoundSwitch/issues/488)

## [v5.5.0](https://github.com/Belphemur/SoundSwitch/tree/v5.5.0) (2020-07-28)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.4.0...v5.5.0)

**Implemented enhancements:**

- ui window position when starting from task bar.. [\#484](https://github.com/Belphemur/SoundSwitch/issues/484)
- Notifications on profile activation [\#483](https://github.com/Belphemur/SoundSwitch/issues/483)
- Reconnected devices and profiles [\#482](https://github.com/Belphemur/SoundSwitch/issues/482)
- Make the installer update silent when using the changelog view [\#480](https://github.com/Belphemur/SoundSwitch/issues/480)
- Integrate an option to automatic switch the sound device when Steam Big Picture ist running [\#478](https://github.com/Belphemur/SoundSwitch/issues/478)
- Better support for MultiScreen: Banner displayed on active screen [\#475](https://github.com/Belphemur/SoundSwitch/issues/475)
- \[Enhancements\] Profile editing [\#431](https://github.com/Belphemur/SoundSwitch/issues/431)
- Default Communication Device support [\#348](https://github.com/Belphemur/SoundSwitch/issues/348)
- Add feature: Switch only communications device [\#252](https://github.com/Belphemur/SoundSwitch/issues/252)

**Fixed bugs:**

- Installer cannot detect correctly version of .NET Core runtime when installed with the SDK [\#486](https://github.com/Belphemur/SoundSwitch/issues/486)
- Crash when trying to display the tooltip of the playback device [\#481](https://github.com/Belphemur/SoundSwitch/issues/481)
- Installation crashes at .NET Core 3.1.4 since Update from Microsoft [\#479](https://github.com/Belphemur/SoundSwitch/issues/479)
- Installer Offers To Install Older Desktop Runtime [\#474](https://github.com/Belphemur/SoundSwitch/issues/474)
- HDMI sound device is not switched to using the hotkey [\#466](https://github.com/Belphemur/SoundSwitch/issues/466)

**Closed issues:**

- SoundSwitch crash when switching profile with a device that isn't active at time [\#485](https://github.com/Belphemur/SoundSwitch/issues/485)
- SounSwitch crashes after launch in Windows 7 x64  [\#477](https://github.com/Belphemur/SoundSwitch/issues/477)
- Crash at startup when soundswitch is installed on multiple users on Windows 10 [\#476](https://github.com/Belphemur/SoundSwitch/issues/476)

## [v5.4.0](https://github.com/Belphemur/SoundSwitch/tree/v5.4.0) (2020-06-10)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.3.1...v5.4.0)

**Implemented enhancements:**

- Make an item in the menu for the Community / Discord [\#472](https://github.com/Belphemur/SoundSwitch/issues/472)
- SoundSwitch - Forum, Discord Chat & FAQs. [\#470](https://github.com/Belphemur/SoundSwitch/issues/470)
- Add Croatian to the app [\#465](https://github.com/Belphemur/SoundSwitch/issues/465)

**Fixed bugs:**

- Banner + acoustic signal when switching output device [\#471](https://github.com/Belphemur/SoundSwitch/issues/471)
- Settings Tab Not Displaying Correctly [\#467](https://github.com/Belphemur/SoundSwitch/issues/467)
- SoundSwitch crash at boot: Instance not initialized [\#464](https://github.com/Belphemur/SoundSwitch/issues/464)
- SoundSwitch crash at start: Named Pipe access denied [\#463](https://github.com/Belphemur/SoundSwitch/issues/463)
- Crash when switching to a profile where the device isn't connected [\#462](https://github.com/Belphemur/SoundSwitch/issues/462)
- SwoundSwitch preventing Windows 10 Sleep mode [\#439](https://github.com/Belphemur/SoundSwitch/issues/439)

**Closed issues:**

- Soundswitch crashes at startup [\#468](https://github.com/Belphemur/SoundSwitch/issues/468)
- Unable to donate via PayPal [\#312](https://github.com/Belphemur/SoundSwitch/issues/312)

**Merged pull requests:**

- feature/ui-improvements: Various small improvements for the UI [\#461](https://github.com/Belphemur/SoundSwitch/pull/461) ([FireEmerald](https://github.com/FireEmerald))

## [v5.3.1](https://github.com/Belphemur/SoundSwitch/tree/v5.3.1) (2020-05-31)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.3.0...v5.3.1)

**Fixed bugs:**

- Trying to get Help/Donate using the menu make SoundSwitch crash [\#460](https://github.com/Belphemur/SoundSwitch/issues/460)

**Merged pull requests:**

- Re-organize controls of the profile form [\#497](https://github.com/Belphemur/SoundSwitch/pull/497) ([FireEmerald](https://github.com/FireEmerald))
- Update SettingsStrings.ru-RU.resx [\#459](https://github.com/Belphemur/SoundSwitch/pull/459) ([wvxwxvw](https://github.com/wvxwxvw))

## [v5.3.0](https://github.com/Belphemur/SoundSwitch/tree/v5.3.0) (2020-05-30)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.2.0...v5.3.0)

**Implemented enhancements:**

- When setting profile using an application, let the user choose if the default audio device is changed too [\#458](https://github.com/Belphemur/SoundSwitch/issues/458)
- Better error message when launching second instance of SoundSwitch [\#456](https://github.com/Belphemur/SoundSwitch/issues/456)

**Fixed bugs:**

- SoundSwitch crashes on startup:  Can't get information about running process for Profile Feature [\#455](https://github.com/Belphemur/SoundSwitch/issues/455)

**Closed issues:**

- Crash after booting up PC: When using Profile feature and the audio device isn't connected [\#457](https://github.com/Belphemur/SoundSwitch/issues/457)

## [v5.2.0](https://github.com/Belphemur/SoundSwitch/tree/v5.2.0) (2020-05-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.1.1...v5.2.0)

**Implemented enhancements:**

- Move to use .NET Core [\#453](https://github.com/Belphemur/SoundSwitch/issues/453)

**Fixed bugs:**

- SoundSwitch doesn't remember when HotKey is disabled [\#451](https://github.com/Belphemur/SoundSwitch/issues/451)
- Recognize installation directory [\#449](https://github.com/Belphemur/SoundSwitch/issues/449)

**Merged pull requests:**

- Net core 3.1 [\#454](https://github.com/Belphemur/SoundSwitch/pull/454) ([Belphemur](https://github.com/Belphemur))

## [v5.1.1](https://github.com/Belphemur/SoundSwitch/tree/v5.1.1) (2020-05-17)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.1.0...v5.1.1)

**Fixed bugs:**

- Upgrade issues: Installer fails to start SW. Hotkey not functional after starting manually [\#447](https://github.com/Belphemur/SoundSwitch/issues/447)
- app crash new beta 5.1.0 [\#446](https://github.com/Belphemur/SoundSwitch/issues/446)

## [v5.1.0](https://github.com/Belphemur/SoundSwitch/tree/v5.1.0) (2020-05-16)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.0.4...v5.1.0)

**Implemented enhancements:**

- Make the Disconnected device list collapsible [\#445](https://github.com/Belphemur/SoundSwitch/issues/445)
- Add Dutch to the application [\#440](https://github.com/Belphemur/SoundSwitch/issues/440)

**Fixed bugs:**

- Recognize re-connected devices as the same as before [\#444](https://github.com/Belphemur/SoundSwitch/issues/444)
- Keybind not working, crashing when trying to edit keybind. [\#443](https://github.com/Belphemur/SoundSwitch/issues/443)

**Closed issues:**

- cra\<sh [\#438](https://github.com/Belphemur/SoundSwitch/issues/438)

## [v5.0.4](https://github.com/Belphemur/SoundSwitch/tree/v5.0.4) (2020-05-10)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.0.3...v5.0.4)

**Implemented enhancements:**

- When deleting a profile reset all changes made by profile to the Windows audio system [\#430](https://github.com/Belphemur/SoundSwitch/issues/430)

**Fixed bugs:**

- Need to close the log file before packing when app crashed [\#434](https://github.com/Belphemur/SoundSwitch/issues/434)
- NullReferenceException when the last playback device is disabled [\#432](https://github.com/Belphemur/SoundSwitch/issues/432)
- Hotkey doesn't register & hotkey bar display issue [\#429](https://github.com/Belphemur/SoundSwitch/issues/429)
- Crashes at startup when all playback devices are disabled [\#427](https://github.com/Belphemur/SoundSwitch/issues/427)
- Duplicate device created/previous removed when reconnected. [\#425](https://github.com/Belphemur/SoundSwitch/issues/425)
- Default Communication Device changing when it shouldn't. [\#418](https://github.com/Belphemur/SoundSwitch/issues/418)

**Merged pull requests:**

- Close the log file before packing to zip [\#435](https://github.com/Belphemur/SoundSwitch/pull/435) ([moomons](https://github.com/moomons))
- Fix NullReferenceException when the last playback device is disabled [\#433](https://github.com/Belphemur/SoundSwitch/pull/433) ([moomons](https://github.com/moomons))
- Fix crash at startup when all playback devices are disabled [\#428](https://github.com/Belphemur/SoundSwitch/pull/428) ([moomons](https://github.com/moomons))
- Reconnected devices are not added back to the selected list [\#426](https://github.com/Belphemur/SoundSwitch/pull/426) ([ys27](https://github.com/ys27))

## [v5.0.3](https://github.com/Belphemur/SoundSwitch/tree/v5.0.3) (2020-04-18)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.0.2...v5.0.3)

**Implemented enhancements:**

- Make the updater detect if installed as User or Admin [\#416](https://github.com/Belphemur/SoundSwitch/issues/416)

**Fixed bugs:**

- Can't use Windows Key as part of the HotKey [\#421](https://github.com/Belphemur/SoundSwitch/issues/421)

## [v5.0.2](https://github.com/Belphemur/SoundSwitch/tree/v5.0.2) (2020-04-04)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.0.1...v5.0.2)

**Fixed bugs:**

- Security Vulnerability in the Updater [\#415](https://github.com/Belphemur/SoundSwitch/issues/415)
- 5.0.1 crashing on first run [\#413](https://github.com/Belphemur/SoundSwitch/issues/413)
- My hot-key stopped working after the last patch [\#408](https://github.com/Belphemur/SoundSwitch/issues/408)
- Install / Upgrade issue [\#404](https://github.com/Belphemur/SoundSwitch/issues/404)
- SoundSwitchAggregateException / error message [\#397](https://github.com/Belphemur/SoundSwitch/issues/397)
- Auto Updater is Crashing on v4.15.\* [\#376](https://github.com/Belphemur/SoundSwitch/issues/376)

**Closed issues:**

- Bug - Cannot set hot keys [\#412](https://github.com/Belphemur/SoundSwitch/issues/412)
- Crash when no audio device available [\#411](https://github.com/Belphemur/SoundSwitch/issues/411)
- url [\#407](https://github.com/Belphemur/SoundSwitch/issues/407)
- Hotkey gets various characters in addition to the key I actually want [\#405](https://github.com/Belphemur/SoundSwitch/issues/405)
- Randomly switch on it own and back when viewing websites, desktop or doing anything  [\#403](https://github.com/Belphemur/SoundSwitch/issues/403)
- SoundSwitch\_v5.0.1.14646\_Release [\#402](https://github.com/Belphemur/SoundSwitch/issues/402)
- Forced mode [\#400](https://github.com/Belphemur/SoundSwitch/issues/400)

## [v5.0.1](https://github.com/Belphemur/SoundSwitch/tree/v5.0.1) (2020-01-25)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.0.0...v5.0.1)

**Fixed bugs:**

- Crash when switching with v5.0.0 [\#401](https://github.com/Belphemur/SoundSwitch/issues/401)

## [v5.0.0](https://github.com/Belphemur/SoundSwitch/tree/v5.0.0) (2020-01-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.17.1...v5.0.0)

**Implemented enhancements:**

- Support install through Chocolatey package manager [\#372](https://github.com/Belphemur/SoundSwitch/issues/372)

**Closed issues:**

- What folder should I backup? [\#399](https://github.com/Belphemur/SoundSwitch/issues/399)
- Error when installing just for me [\#398](https://github.com/Belphemur/SoundSwitch/issues/398)

## [v4.17.1](https://github.com/Belphemur/SoundSwitch/tree/v4.17.1) (2019-12-26)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.17.0...v4.17.1)

**Implemented enhancements:**

- Combined hotkey for In+Output device [\#110](https://github.com/Belphemur/SoundSwitch/issues/110)

**Fixed bugs:**

- Unable to create a profile without linked application [\#395](https://github.com/Belphemur/SoundSwitch/issues/395)

## [v4.17.0](https://github.com/Belphemur/SoundSwitch/tree/v4.17.0) (2019-12-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.16.3...v4.17.0)

**Implemented enhancements:**

- Add Korean language [\#391](https://github.com/Belphemur/SoundSwitch/issues/391)
- A hotkey to switch audio devices only for specific applications [\#257](https://github.com/Belphemur/SoundSwitch/issues/257)
- Possible to make a portable install? [\#208](https://github.com/Belphemur/SoundSwitch/issues/208)
- Add device profiles tab, and add new hotkey combo to switch between them [\#207](https://github.com/Belphemur/SoundSwitch/issues/207)

**Fixed bugs:**

- Unable to set custom sound notification [\#386](https://github.com/Belphemur/SoundSwitch/issues/386)
- Soundswitch crash on startup [\#214](https://github.com/Belphemur/SoundSwitch/issues/214)

**Closed issues:**

- Latest version won't install [\#388](https://github.com/Belphemur/SoundSwitch/issues/388)

**Merged pull requests:**

- Feature profile [\#393](https://github.com/Belphemur/SoundSwitch/pull/393) ([Belphemur](https://github.com/Belphemur))

## [v4.16.3](https://github.com/Belphemur/SoundSwitch/tree/v4.16.3) (2019-12-14)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.16.2...v4.16.3)

**Closed issues:**

- Shortcut error [\#387](https://github.com/Belphemur/SoundSwitch/issues/387)
- Russian language [\#384](https://github.com/Belphemur/SoundSwitch/issues/384)
- Crash report [\#383](https://github.com/Belphemur/SoundSwitch/issues/383)
- Installer for 14.6.2.3656 fails [\#382](https://github.com/Belphemur/SoundSwitch/issues/382)

**Merged pull requests:**

- Update SettingsStrings.ru-RU.resx [\#385](https://github.com/Belphemur/SoundSwitch/pull/385) ([wvxwxvw](https://github.com/wvxwxvw))

## [v4.16.2](https://github.com/Belphemur/SoundSwitch/tree/v4.16.2) (2019-11-26)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.16.1...v4.16.2)

**Fixed bugs:**

- The option "Change Icon" is misleading and not self-explanatory [\#374](https://github.com/Belphemur/SoundSwitch/issues/374)

**Closed issues:**

- Access violation error when updating [\#375](https://github.com/Belphemur/SoundSwitch/issues/375)

**Merged pull requests:**

- Improved English readme and synced with German [\#396](https://github.com/Belphemur/SoundSwitch/pull/396) ([FireEmerald](https://github.com/FireEmerald))
- Polish translation added [\#381](https://github.com/Belphemur/SoundSwitch/pull/381) ([ArquesMartin](https://github.com/ArquesMartin))
- Update AboutStrings.ru-RU.resx [\#380](https://github.com/Belphemur/SoundSwitch/pull/380) ([wvxwxvw](https://github.com/wvxwxvw))
- Update SettingsStrings.ru-RU.resx [\#379](https://github.com/Belphemur/SoundSwitch/pull/379) ([wvxwxvw](https://github.com/wvxwxvw))
- Update TrayIconStrings.ru-RU.resx [\#378](https://github.com/Belphemur/SoundSwitch/pull/378) ([wvxwxvw](https://github.com/wvxwxvw))
- Update UpdateDownloadStrings.ru-RU.resx [\#377](https://github.com/Belphemur/SoundSwitch/pull/377) ([wvxwxvw](https://github.com/wvxwxvw))

## [v4.16.1](https://github.com/Belphemur/SoundSwitch/tree/v4.16.1) (2019-11-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.16.0...v4.16.1)

**Implemented enhancements:**

- Icon can change when any device is switched [\#373](https://github.com/Belphemur/SoundSwitch/issues/373)
- Possibility to link application to Audio Device [\#13](https://github.com/Belphemur/SoundSwitch/issues/13)

## [v4.16.0](https://github.com/Belphemur/SoundSwitch/tree/v4.16.0) (2019-11-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.15.1...v4.16.0)

**Implemented enhancements:**

- User can choose how the SoundSwitch Icon changes [\#368](https://github.com/Belphemur/SoundSwitch/issues/368)
- Would you like a translated readme in German? [\#366](https://github.com/Belphemur/SoundSwitch/issues/366)

**Fixed bugs:**

- SoundSwitch crashes when switching to "Customized Sound Notification" [\#365](https://github.com/Belphemur/SoundSwitch/issues/365)
- Possible Windows/Nvidia update causing persistent 4.10 and 4.15 issues. [\#363](https://github.com/Belphemur/SoundSwitch/issues/363)
- Speakers registering as disconnected in SoundSwitch since update 4.15.0.40592 [\#361](https://github.com/Belphemur/SoundSwitch/issues/361)
- Working device showing as "Disconnected" after latest update [\#360](https://github.com/Belphemur/SoundSwitch/issues/360)
- Shortcut Not Working [\#359](https://github.com/Belphemur/SoundSwitch/issues/359)
- Missing Audio Device [\#358](https://github.com/Belphemur/SoundSwitch/issues/358)
- Missing second playback device with same name [\#338](https://github.com/Belphemur/SoundSwitch/issues/338)

**Closed issues:**

- Certificates are missing in the repository [\#367](https://github.com/Belphemur/SoundSwitch/issues/367)
- Cannot Switch after the latest update [\#364](https://github.com/Belphemur/SoundSwitch/issues/364)
- Bluetooth Speakers Not Shown [\#362](https://github.com/Belphemur/SoundSwitch/issues/362)
- Switching Audio Devices Frequently Leads To Audio Output From Multiple Audio Devices [\#356](https://github.com/Belphemur/SoundSwitch/issues/356)
- Request: custom volume per device [\#333](https://github.com/Belphemur/SoundSwitch/issues/333)
- Feature Request: Delay Launch for SoundSwitch [\#332](https://github.com/Belphemur/SoundSwitch/issues/332)

**Merged pull requests:**

- Added german README, based on translations from Overload86 [\#369](https://github.com/Belphemur/SoundSwitch/pull/369) ([FireEmerald](https://github.com/FireEmerald))

## [v4.15.1](https://github.com/Belphemur/SoundSwitch/tree/v4.15.1) (2019-11-13)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.15.0...v4.15.1)

**Fixed bugs:**

- Cycle throu selected  not working [\#357](https://github.com/Belphemur/SoundSwitch/issues/357)

## [v4.15.0](https://github.com/Belphemur/SoundSwitch/tree/v4.15.0) (2019-11-12)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.14.0...v4.15.0)

**Implemented enhancements:**

- The installer dumps a setup log file next to itself after normal installation [\#337](https://github.com/Belphemur/SoundSwitch/issues/337)
- Don't launch the application after a silent install [\#336](https://github.com/Belphemur/SoundSwitch/issues/336)
- Switch the foreground app also \[WIN10\] [\#334](https://github.com/Belphemur/SoundSwitch/issues/334)

**Fixed bugs:**

- Incorrect tray icon displayed [\#340](https://github.com/Belphemur/SoundSwitch/issues/340)

**Closed issues:**

- Sound Switch is not opening after the newest windows update \(1903\) [\#355](https://github.com/Belphemur/SoundSwitch/issues/355)
- Soundswitch shortcut doesn't work anymore [\#354](https://github.com/Belphemur/SoundSwitch/issues/354)
- Switch audio outputs when SoundSwitch is run while already running. [\#353](https://github.com/Belphemur/SoundSwitch/issues/353)
- \[Feature request\] option to hide tray icon [\#352](https://github.com/Belphemur/SoundSwitch/issues/352)
- This program messed up my sound settings after uninstalling [\#350](https://github.com/Belphemur/SoundSwitch/issues/350)
- Unable to download installer - SSL Issues [\#349](https://github.com/Belphemur/SoundSwitch/issues/349)
- Hotkey not working [\#347](https://github.com/Belphemur/SoundSwitch/issues/347)
- \[Feature Request\] Switch output by a shortcut without SoundSwitch running in tray [\#346](https://github.com/Belphemur/SoundSwitch/issues/346)
- Icon in start menu is small when medium icon size is selected [\#345](https://github.com/Belphemur/SoundSwitch/issues/345)
- Not working in Chrome [\#344](https://github.com/Belphemur/SoundSwitch/issues/344)
- Default audio is switching but my opened programs do not. [\#343](https://github.com/Belphemur/SoundSwitch/issues/343)
- Audio out of only one source [\#342](https://github.com/Belphemur/SoundSwitch/issues/342)
- Microphone level is reset to 0 after switching [\#339](https://github.com/Belphemur/SoundSwitch/issues/339)
- Not switching audio devices without restarting program with bootup on start enabled [\#335](https://github.com/Belphemur/SoundSwitch/issues/335)

## [v4.14.0](https://github.com/Belphemur/SoundSwitch/tree/v4.14.0) (2019-04-19)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.13.0...v4.14.0)

**Closed issues:**

- \[Bug\] No Logitech G533 after updating soundswitch to 4.13 [\#331](https://github.com/Belphemur/SoundSwitch/issues/331)

## [v4.13.0](https://github.com/Belphemur/SoundSwitch/tree/v4.13.0) (2019-03-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.12...v4.13.0)

**Fixed bugs:**

- High CPU came back [\#330](https://github.com/Belphemur/SoundSwitch/issues/330)
- Soundswitch crash at start when no internet [\#326](https://github.com/Belphemur/SoundSwitch/issues/326)
- Hotkeys do not work even after 4.12 [\#324](https://github.com/Belphemur/SoundSwitch/issues/324)

**Closed issues:**

- Link Playback and Recording devices together [\#328](https://github.com/Belphemur/SoundSwitch/issues/328)
- Installer crashing [\#327](https://github.com/Belphemur/SoundSwitch/issues/327)
- \[Feature\] Switch device on single click on tray icon [\#325](https://github.com/Belphemur/SoundSwitch/issues/325)
- Characters in french are not displayed correctly in the installer [\#313](https://github.com/Belphemur/SoundSwitch/issues/313)
- SoundSwitch not launching on Windows 8.1 [\#136](https://github.com/Belphemur/SoundSwitch/issues/136)
- Problem with VC Redist and Win 10 anniversary update [\#114](https://github.com/Belphemur/SoundSwitch/issues/114)

## [v4.12](https://github.com/Belphemur/SoundSwitch/tree/v4.12) (2019-02-26)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.11...v4.12)

**Fixed bugs:**

- SoundSwitch does not refresh itself when connecting new device to computer [\#323](https://github.com/Belphemur/SoundSwitch/issues/323)
- Toast Notification not working [\#321](https://github.com/Belphemur/SoundSwitch/issues/321)

**Closed issues:**

- Not switching to HDMI Audio Device [\#322](https://github.com/Belphemur/SoundSwitch/issues/322)
- Failure to install Visual C++ Redistributable during first installation. [\#320](https://github.com/Belphemur/SoundSwitch/issues/320)
- Breaks Spotify [\#318](https://github.com/Belphemur/SoundSwitch/issues/318)
- Soundswitch does not open itself at boot. [\#317](https://github.com/Belphemur/SoundSwitch/issues/317)
- Visual C++ Redistributable for Visual Studio 2017 issues with  other program [\#315](https://github.com/Belphemur/SoundSwitch/issues/315)
- 4.11.69 Bug with hotkeys [\#314](https://github.com/Belphemur/SoundSwitch/issues/314)

## [v4.11](https://github.com/Belphemur/SoundSwitch/tree/v4.11) (2018-12-05)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.10...v4.11)

**Implemented enhancements:**

- Add Chinese language [\#311](https://github.com/Belphemur/SoundSwitch/issues/311)

**Fixed bugs:**

- High CPU usage with 4.10.6899.14921 when coming back from hibernation/sleep [\#309](https://github.com/Belphemur/SoundSwitch/issues/309)

## [v4.10](https://github.com/Belphemur/SoundSwitch/tree/v4.10) (2018-11-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.9...v4.10)

**Fixed bugs:**

- Lag opening system tray context menu [\#298](https://github.com/Belphemur/SoundSwitch/issues/298)
- 4.9 Update will not finish [\#294](https://github.com/Belphemur/SoundSwitch/issues/294)
- Crashing, unstableness and messed up dialogues. [\#293](https://github.com/Belphemur/SoundSwitch/issues/293)
- SoundSwitch crashing ... again [\#292](https://github.com/Belphemur/SoundSwitch/issues/292)

**Closed issues:**

- Default Communications Device [\#308](https://github.com/Belphemur/SoundSwitch/issues/308)
- I can not install or update SoundSwitch. [\#307](https://github.com/Belphemur/SoundSwitch/issues/307)
- no way to set a shotcut [\#305](https://github.com/Belphemur/SoundSwitch/issues/305)
- Crashing at launch [\#303](https://github.com/Belphemur/SoundSwitch/issues/303)
- Odd issue with soundswitch [\#302](https://github.com/Belphemur/SoundSwitch/issues/302)
- Enhancement - Leftclick to switch devices [\#301](https://github.com/Belphemur/SoundSwitch/issues/301)
- Soundswitch App Wont Open  [\#296](https://github.com/Belphemur/SoundSwitch/issues/296)
- Feat.Req.: create and switch-to "profiles" \(e.g. Docked-Softphone, Docked-VideoConf\) [\#295](https://github.com/Belphemur/SoundSwitch/issues/295)
- Soundswitch won't appear on the screen even thought it is running in the task manager [\#243](https://github.com/Belphemur/SoundSwitch/issues/243)
- SoundSwitch is crashing frequently [\#232](https://github.com/Belphemur/SoundSwitch/issues/232)

**Merged pull requests:**

- Add license scan report and status [\#300](https://github.com/Belphemur/SoundSwitch/pull/300) ([fossabot](https://github.com/fossabot))

## [v4.9](https://github.com/Belphemur/SoundSwitch/tree/v4.9) (2018-06-09)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.8...v4.9)

**Fixed bugs:**

- New logo mostly invisible on white background [\#287](https://github.com/Belphemur/SoundSwitch/issues/287)
- Switching between Audio sources delayed - v4.8 [\#286](https://github.com/Belphemur/SoundSwitch/issues/286)
- SoundSwitch not starting after installation finished [\#276](https://github.com/Belphemur/SoundSwitch/issues/276)

**Closed issues:**

- Much slower switching ever since 4.8 [\#291](https://github.com/Belphemur/SoundSwitch/issues/291)
- Interface slow to respond to click to open [\#288](https://github.com/Belphemur/SoundSwitch/issues/288)

**Merged pull requests:**

- Fix performance [\#290](https://github.com/Belphemur/SoundSwitch/pull/290) ([Belphemur](https://github.com/Belphemur))
- Fix Icon [\#289](https://github.com/Belphemur/SoundSwitch/pull/289) ([Belphemur](https://github.com/Belphemur))
- Bump version [\#285](https://github.com/Belphemur/SoundSwitch/pull/285) ([Belphemur](https://github.com/Belphemur))

## [v4.8](https://github.com/Belphemur/SoundSwitch/tree/v4.8) (2018-06-06)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.7...v4.8)

**Implemented enhancements:**

- Fallback on Name when Id not matching [\#283](https://github.com/Belphemur/SoundSwitch/issues/283)
- Modern Logo for Soundswitch [\#277](https://github.com/Belphemur/SoundSwitch/issues/277)

**Closed issues:**

- Remove duplicate trayicon file [\#281](https://github.com/Belphemur/SoundSwitch/issues/281)
- How to test my forked Soundswitch-dev installer? [\#273](https://github.com/Belphemur/SoundSwitch/issues/273)
- Crash on boot after motherboard change [\#251](https://github.com/Belphemur/SoundSwitch/issues/251)

**Merged pull requests:**

- Fresh new look for README [\#371](https://github.com/Belphemur/SoundSwitch/pull/371) ([FireEmerald](https://github.com/FireEmerald))
- Name fallback [\#284](https://github.com/Belphemur/SoundSwitch/pull/284) ([Belphemur](https://github.com/Belphemur))
- Cleaned and updated Makefiles [\#280](https://github.com/Belphemur/SoundSwitch/pull/280) ([FireEmerald](https://github.com/FireEmerald))
- Added logo and ico [\#278](https://github.com/Belphemur/SoundSwitch/pull/278) ([linadesteem](https://github.com/linadesteem))
- Add Portuguese\(Brazilian\) to the installer. [\#275](https://github.com/Belphemur/SoundSwitch/pull/275) ([aleczk](https://github.com/aleczk))

## [v4.7](https://github.com/Belphemur/SoundSwitch/tree/v4.7) (2018-05-15)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.6...v4.7)

**Fixed bugs:**

- Crashing when mouse over tray icon [\#272](https://github.com/Belphemur/SoundSwitch/issues/272)
- Readme missing screenshot [\#271](https://github.com/Belphemur/SoundSwitch/issues/271)

**Closed issues:**

- Sound Switch keeps closing [\#270](https://github.com/Belphemur/SoundSwitch/issues/270)
- SoundSwitch isn't working with chrome sometimes [\#269](https://github.com/Belphemur/SoundSwitch/issues/269)

## [v4.6](https://github.com/Belphemur/SoundSwitch/tree/v4.6) (2018-05-13)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.5...v4.6)

**Fixed bugs:**

- App randomly crashes, every 2 hours or so, with latest 4.5 update.  [\#268](https://github.com/Belphemur/SoundSwitch/issues/268)
- Double Click to switch output device crashes app [\#267](https://github.com/Belphemur/SoundSwitch/issues/267)

**Closed issues:**

- With default communications checked, it would not switch properly [\#256](https://github.com/Belphemur/SoundSwitch/issues/256)

**Merged pull requests:**

- Update SettingsStrings.pt-BR.resx [\#266](https://github.com/Belphemur/SoundSwitch/pull/266) ([opus-2](https://github.com/opus-2))
- Update italian.iss [\#265](https://github.com/Belphemur/SoundSwitch/pull/265) ([bovirus](https://github.com/bovirus))
- Several updates [\#264](https://github.com/Belphemur/SoundSwitch/pull/264) ([bovirus](https://github.com/bovirus))
- Update Italian strings [\#263](https://github.com/Belphemur/SoundSwitch/pull/263) ([bovirus](https://github.com/bovirus))

## [v4.5](https://github.com/Belphemur/SoundSwitch/tree/v4.5) (2018-05-12)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.4...v4.5)

**Implemented enhancements:**

- Add option to disable selected sound on device switch [\#261](https://github.com/Belphemur/SoundSwitch/issues/261)

**Merged pull requests:**

- Maybe fix memory leak caused by not disposing Icons and Bitmaps. [\#262](https://github.com/Belphemur/SoundSwitch/pull/262) ([Niko-O](https://github.com/Niko-O))
- Update ISS file \(add Italian language\) [\#260](https://github.com/Belphemur/SoundSwitch/pull/260) ([bovirus](https://github.com/bovirus))

## [v4.4](https://github.com/Belphemur/SoundSwitch/tree/v4.4) (2018-05-09)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.3...v4.4)

**Implemented enhancements:**

- New Language: Portuguese \(Brazilian\) - Pull Request [\#258](https://github.com/Belphemur/SoundSwitch/issues/258)
- Improve logging [\#254](https://github.com/Belphemur/SoundSwitch/issues/254)

**Fixed bugs:**

- Crash observed after restart driven by Windows Update [\#248](https://github.com/Belphemur/SoundSwitch/issues/248)
- In x86, the installer always force to install VC++ Redist [\#245](https://github.com/Belphemur/SoundSwitch/issues/245)

**Closed issues:**

- Identify Re-Connected Displays /w Audio [\#250](https://github.com/Belphemur/SoundSwitch/issues/250)

**Merged pull requests:**

- Improve logging [\#259](https://github.com/Belphemur/SoundSwitch/pull/259) ([Belphemur](https://github.com/Belphemur))
- Norwegian language [\#255](https://github.com/Belphemur/SoundSwitch/pull/255) ([Belphemur](https://github.com/Belphemur))
- Added Spanish language in installer [\#246](https://github.com/Belphemur/SoundSwitch/pull/246) ([plextoriano](https://github.com/plextoriano))

## [v4.3](https://github.com/Belphemur/SoundSwitch/tree/v4.3) (2018-03-10)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.2...v4.3)

**Closed issues:**

- Unable to set shortcut [\#241](https://github.com/Belphemur/SoundSwitch/issues/241)
- Different Hotkey per device? [\#238](https://github.com/Belphemur/SoundSwitch/issues/238)
- Switch Device by system time [\#235](https://github.com/Belphemur/SoundSwitch/issues/235)

## [v4.2](https://github.com/Belphemur/SoundSwitch/tree/v4.2) (2017-12-03)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.1...v4.2)

**Implemented enhancements:**

- Rewrite of the core of SoundSwitch [\#224](https://github.com/Belphemur/SoundSwitch/issues/224)

**Fixed bugs:**

- Race condition of the WindowsAPIThread leading to crash on start. [\#229](https://github.com/Belphemur/SoundSwitch/issues/229)

## [v4.1](https://github.com/Belphemur/SoundSwitch/tree/v4.1) (2017-11-30)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.0...v4.1)

**Implemented enhancements:**

- Add choice in uninstaller to remove application configuration [\#216](https://github.com/Belphemur/SoundSwitch/issues/216)

**Fixed bugs:**

- Settings crash when device don't have a friendly name [\#225](https://github.com/Belphemur/SoundSwitch/issues/225)
- Locking up on switch since Fall Creator's Update [\#219](https://github.com/Belphemur/SoundSwitch/issues/219)
- Application crashes when activating a bluetooth device - Win7 [\#217](https://github.com/Belphemur/SoundSwitch/issues/217)
- SoundSwitch Freezing randomly. [\#200](https://github.com/Belphemur/SoundSwitch/issues/200)
- New-Old Issue - Switching to Bluetooth Audio [\#166](https://github.com/Belphemur/SoundSwitch/issues/166)

**Merged pull requests:**

- Rewrite [\#226](https://github.com/Belphemur/SoundSwitch/pull/226) ([Belphemur](https://github.com/Belphemur))

## [v4.0](https://github.com/Belphemur/SoundSwitch/tree/v4.0) (2017-11-26)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.15.2...v4.0)

## [v3.15.2](https://github.com/Belphemur/SoundSwitch/tree/v3.15.2) (2017-11-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.15.1...v3.15.2)

**Implemented enhancements:**

- Add ability to link output/recording devices [\#206](https://github.com/Belphemur/SoundSwitch/issues/206)

**Fixed bugs:**

- App Crash with MP3 file used for Banner Notification \(works fine in Custom Sound Notification\) [\#218](https://github.com/Belphemur/SoundSwitch/issues/218)
- Invalid sound file make SoundSwitch crash [\#210](https://github.com/Belphemur/SoundSwitch/issues/210)

**Closed issues:**

- SoundSwitch won't launch [\#205](https://github.com/Belphemur/SoundSwitch/issues/205)
- Crashes when remote connecting to machine using RDP [\#197](https://github.com/Belphemur/SoundSwitch/issues/197)

## [v3.15.1](https://github.com/Belphemur/SoundSwitch/tree/v3.15.1) (2017-07-08)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.15.0...v3.15.1)

**Fixed bugs:**

- Can't install vcredist with the built in installer provided by soundswitch [\#203](https://github.com/Belphemur/SoundSwitch/issues/203)
- SoundSwitch in Homeoffice/Office [\#199](https://github.com/Belphemur/SoundSwitch/issues/199)
- Updater Progress bar crash with Classic Visual Theme [\#194](https://github.com/Belphemur/SoundSwitch/issues/194)

**Closed issues:**

- Crash when not selecting a file for the custom sound notification option [\#202](https://github.com/Belphemur/SoundSwitch/issues/202)
- High CPU usage because of "Windows Audio Device Graph Isolation" process [\#198](https://github.com/Belphemur/SoundSwitch/issues/198)
- Error popup when trying to install latest update [\#196](https://github.com/Belphemur/SoundSwitch/issues/196)
- Branding version for GTribe [\#148](https://github.com/Belphemur/SoundSwitch/issues/148)

**Merged pull requests:**

- Spanish language added [\#244](https://github.com/Belphemur/SoundSwitch/pull/244) ([plextoriano](https://github.com/plextoriano))
- Add VC Redist 2017 as dependency [\#204](https://github.com/Belphemur/SoundSwitch/pull/204) ([Belphemur](https://github.com/Belphemur))
- Fixed a crash which happened if the user disabled visual styles e.g. with the 'Windows Classic' theme of Windows 7. [\#195](https://github.com/Belphemur/SoundSwitch/pull/195) ([FireEmerald](https://github.com/FireEmerald))

## [v3.15.0](https://github.com/Belphemur/SoundSwitch/tree/v3.15.0) (2017-05-31)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.14.2...v3.15.0)

**Implemented enhancements:**

- Make the banner notification use the image of the Device [\#192](https://github.com/Belphemur/SoundSwitch/issues/192)
- Update German [\#189](https://github.com/Belphemur/SoundSwitch/issues/189)
- Unable to bind ScrollLock as a hotkey [\#151](https://github.com/Belphemur/SoundSwitch/issues/151)

**Fixed bugs:**

- Installer don't detect .net 4.7 [\#193](https://github.com/Belphemur/SoundSwitch/issues/193)
- Soundswitch crashes on RDP connect [\#187](https://github.com/Belphemur/SoundSwitch/issues/187)

**Closed issues:**

- Recording device hotkey doesn't cycle [\#191](https://github.com/Belphemur/SoundSwitch/issues/191)

**Merged pull requests:**

- Translations [\#190](https://github.com/Belphemur/SoundSwitch/pull/190) ([Belphemur](https://github.com/Belphemur))

## [v3.14.2](https://github.com/Belphemur/SoundSwitch/tree/v3.14.2) (2017-05-25)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.14.1...v3.14.2)

**Fixed bugs:**

- Unresponsive after RDC [\#112](https://github.com/Belphemur/SoundSwitch/issues/112)

**Closed issues:**

- Error after Creator's Update [\#185](https://github.com/Belphemur/SoundSwitch/issues/185)
- Kaspersky Internet Security detects Virus and crashes SoundSwitch while updating \(false-positive\) [\#180](https://github.com/Belphemur/SoundSwitch/issues/180)

**Merged pull requests:**

- Add thanks category [\#188](https://github.com/Belphemur/SoundSwitch/pull/188) ([Belphemur](https://github.com/Belphemur))
- Some improvements and a feature [\#186](https://github.com/Belphemur/SoundSwitch/pull/186) ([ramon18](https://github.com/ramon18))

## [v3.14.1](https://github.com/Belphemur/SoundSwitch/tree/v3.14.1) (2017-04-08)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.14.0...v3.14.1)

**Implemented enhancements:**

- Update AudioEndPointLibrary [\#178](https://github.com/Belphemur/SoundSwitch/issues/178)

**Fixed bugs:**

- SoundSwitch Crash when Custom Notification is set and no Sound is chosen: System.FormatException Fatal Error [\#175](https://github.com/Belphemur/SoundSwitch/issues/175)

**Closed issues:**

- Program keeps crashing if notification setting is set to "Customized Sound Notification" and no sound file is chosen.  [\#177](https://github.com/Belphemur/SoundSwitch/issues/177)
- SoundSwitch doesn't start - Windows 8.1 [\#176](https://github.com/Belphemur/SoundSwitch/issues/176)

## [v3.14.0](https://github.com/Belphemur/SoundSwitch/tree/v3.14.0) (2017-03-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.13.2...v3.14.0)

**Implemented enhancements:**

- Improve the notifcation of new update [\#168](https://github.com/Belphemur/SoundSwitch/issues/168)
- Corrected anchors, added high-dpi support. [\#169](https://github.com/Belphemur/SoundSwitch/pull/169) ([FireEmerald](https://github.com/FireEmerald))

**Fixed bugs:**

- Notification BallonTip not showing [\#173](https://github.com/Belphemur/SoundSwitch/issues/173)
- Settings dialog resize issues [\#164](https://github.com/Belphemur/SoundSwitch/issues/164)

**Closed issues:**

- Kaspersky Internet Security blocks download and execution of latest versoin [\#181](https://github.com/Belphemur/SoundSwitch/issues/181)

## [v3.13.2](https://github.com/Belphemur/SoundSwitch/tree/v3.13.2) (2017-03-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.13.1...v3.13.2)

**Implemented enhancements:**

- Rename the Installer EXE when autoupdate to something more meaningful [\#167](https://github.com/Belphemur/SoundSwitch/issues/167)

**Fixed bugs:**

- Problem with hotkey since new version [\#171](https://github.com/Belphemur/SoundSwitch/issues/171)
- Hotkey issue [\#170](https://github.com/Belphemur/SoundSwitch/issues/170)

**Closed issues:**

- Crashing every time [\#165](https://github.com/Belphemur/SoundSwitch/issues/165)

## [v3.13.1](https://github.com/Belphemur/SoundSwitch/tree/v3.13.1) (2017-03-11)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.13.0...v3.13.1)

**Implemented enhancements:**

- Translate missing strings in German [\#159](https://github.com/Belphemur/SoundSwitch/issues/159)

**Fixed bugs:**

- Crash on Launch, Win7 x64 [\#161](https://github.com/Belphemur/SoundSwitch/issues/161)

**Merged pull requests:**

- Assigned values to DeviceCyclerTypeEnum and NotificationTypeEnum entries. [\#162](https://github.com/Belphemur/SoundSwitch/pull/162) ([FireEmerald](https://github.com/FireEmerald))
- Translated some strings into German introduced in 4dedc42. [\#160](https://github.com/Belphemur/SoundSwitch/pull/160) ([FireEmerald](https://github.com/FireEmerald))

## [v3.13.0](https://github.com/Belphemur/SoundSwitch/tree/v3.13.0) (2017-03-04)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.8...v3.13.0)

**Implemented enhancements:**

- Improve french translation [\#158](https://github.com/Belphemur/SoundSwitch/issues/158)
- Localization improvements [\#156](https://github.com/Belphemur/SoundSwitch/issues/156)
- Ability to disable auto update checker [\#102](https://github.com/Belphemur/SoundSwitch/issues/102)

**Fixed bugs:**

- Tray icon doesn't update until switched  [\#154](https://github.com/Belphemur/SoundSwitch/issues/154)

**Closed issues:**

- Resolve AppVeyor problems [\#153](https://github.com/Belphemur/SoundSwitch/issues/153)

**Merged pull requests:**

- Improved the multi-language support, this includes: [\#157](https://github.com/Belphemur/SoundSwitch/pull/157) ([FireEmerald](https://github.com/FireEmerald))
- Improved the update system to support three different modes [\#152](https://github.com/Belphemur/SoundSwitch/pull/152) ([FireEmerald](https://github.com/FireEmerald))

## [v3.12.8](https://github.com/Belphemur/SoundSwitch/tree/v3.12.8) (2017-02-15)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.7...v3.12.8)

**Implemented enhancements:**

- Use radio button for the Auto-Update [\#150](https://github.com/Belphemur/SoundSwitch/issues/150)

## [v3.12.7](https://github.com/Belphemur/SoundSwitch/tree/v3.12.7) (2017-02-14)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.6...v3.12.7)

**Implemented enhancements:**

- Check the update is signed with a trusted key [\#147](https://github.com/Belphemur/SoundSwitch/issues/147)
- Make the installer install the certificate used by SoundSwitch [\#145](https://github.com/Belphemur/SoundSwitch/issues/145)
- When stealth update is activated, don't auto-update if there is an app in fullscreen [\#144](https://github.com/Belphemur/SoundSwitch/issues/144)

**Fixed bugs:**

- Unable to launch on Windows 7 32bit: Universal C Runtime missing [\#155](https://github.com/Belphemur/SoundSwitch/issues/155)
- Updater crash with Full updater and progress bar [\#83](https://github.com/Belphemur/SoundSwitch/issues/83)

## [v3.12.6](https://github.com/Belphemur/SoundSwitch/tree/v3.12.6) (2017-02-11)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.5...v3.12.6)

**Fixed bugs:**

- Crashed when Bluetooth headphones conneted [\#140](https://github.com/Belphemur/SoundSwitch/issues/140)

**Closed issues:**

- Add donation url in the installer [\#142](https://github.com/Belphemur/SoundSwitch/issues/142)
- Similar to \#140 - Crashes when CONNECTING Bluetooth Device/Headphones [\#141](https://github.com/Belphemur/SoundSwitch/issues/141)

## [v3.12.5](https://github.com/Belphemur/SoundSwitch/tree/v3.12.5) (2017-01-02)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.4...v3.12.5)

**Merged pull requests:**

- TrayIcon is correctly selected after startup [\#134](https://github.com/Belphemur/SoundSwitch/pull/134) ([patgrosse](https://github.com/patgrosse))

## [v3.12.4](https://github.com/Belphemur/SoundSwitch/tree/v3.12.4) (2016-12-09)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.3...v3.12.4)

**Implemented enhancements:**

- When on Beta channel, also install releases [\#132](https://github.com/Belphemur/SoundSwitch/issues/132)

**Fixed bugs:**

- Audio device's selection menu on the systray stopped updating/working [\#131](https://github.com/Belphemur/SoundSwitch/issues/131)

## [v3.12.3](https://github.com/Belphemur/SoundSwitch/tree/v3.12.3) (2016-12-08)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.2...v3.12.3)

**Implemented enhancements:**

- Setting to keep the normal SoundSwitch TrayIcon [\#128](https://github.com/Belphemur/SoundSwitch/issues/128)

**Closed issues:**

- Hotkey to switch playback no longer works [\#130](https://github.com/Belphemur/SoundSwitch/issues/130)

## [v3.12.2](https://github.com/Belphemur/SoundSwitch/tree/v3.12.2) (2016-12-01)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.1...v3.12.2)

**Implemented enhancements:**

- Add a donate button [\#127](https://github.com/Belphemur/SoundSwitch/issues/127)

**Fixed bugs:**

- Reinit the systray icon on Keep Systray setting change [\#135](https://github.com/Belphemur/SoundSwitch/issues/135)
- Installer doesn't check for the right version of VC Redist 2015 [\#126](https://github.com/Belphemur/SoundSwitch/issues/126)
- Device Icon used in Settings is the small one [\#125](https://github.com/Belphemur/SoundSwitch/issues/125)
- App crashes on launch [\#124](https://github.com/Belphemur/SoundSwitch/issues/124)

## [v3.12.1](https://github.com/Belphemur/SoundSwitch/tree/v3.12.1) (2016-11-25)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.0...v3.12.1)

**Implemented enhancements:**

- Device-dependent system tray icon [\#122](https://github.com/Belphemur/SoundSwitch/issues/122)

**Fixed bugs:**

- SoundSwitch crash when stopping [\#123](https://github.com/Belphemur/SoundSwitch/issues/123)

## [v3.12.0](https://github.com/Belphemur/SoundSwitch/tree/v3.12.0) (2016-09-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.11.0...v3.12.0)

**Implemented enhancements:**

- Update to .NET 4.6.2 [\#118](https://github.com/Belphemur/SoundSwitch/issues/118)
- Accessing 'help' at any time [\#105](https://github.com/Belphemur/SoundSwitch/issues/105)
- Request: Notification pop up with custom sound? [\#73](https://github.com/Belphemur/SoundSwitch/issues/73)

**Fixed bugs:**

- Don't switch to already default device [\#117](https://github.com/Belphemur/SoundSwitch/issues/117)

**Closed issues:**

- beta 3.11.0.32914 [\#115](https://github.com/Belphemur/SoundSwitch/issues/115)

## [v3.11.0](https://github.com/Belphemur/SoundSwitch/tree/v3.11.0) (2016-08-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.10.2...v3.11.0)

**Implemented enhancements:**

- Request: WinKey as modifier [\#109](https://github.com/Belphemur/SoundSwitch/issues/109)

**Fixed bugs:**

- Not switching "default communications device" [\#106](https://github.com/Belphemur/SoundSwitch/issues/106)

**Closed issues:**

- Windows 10: Shorter notification duration when 'browsing' with hotkey [\#108](https://github.com/Belphemur/SoundSwitch/issues/108)
- Nevermind [\#104](https://github.com/Belphemur/SoundSwitch/issues/104)

## [v3.10.2](https://github.com/Belphemur/SoundSwitch/tree/v3.10.2) (2016-06-05)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.10.1...v3.10.2)

**Fixed bugs:**

- C++ 2015 x64 redist installed but installer tries and fails to download and install it again [\#101](https://github.com/Belphemur/SoundSwitch/issues/101)

## [v3.10.1](https://github.com/Belphemur/SoundSwitch/tree/v3.10.1) (2016-05-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.10.0...v3.10.1)

**Implemented enhancements:**

- Support HTTPS download in the installer [\#99](https://github.com/Belphemur/SoundSwitch/issues/99)

**Fixed bugs:**

- SoundSwitch install not working, VCRedist detect and download problem v3.10 Stable [\#98](https://github.com/Belphemur/SoundSwitch/issues/98)

## [v3.10.0](https://github.com/Belphemur/SoundSwitch/tree/v3.10.0) (2016-05-15)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.9...v3.10.0)

**Implemented enhancements:**

- High Resolution Icons [\#94](https://github.com/Belphemur/SoundSwitch/issues/94)

## [v3.9.9](https://github.com/Belphemur/SoundSwitch/tree/v3.9.9) (2016-05-01)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.8...v3.9.9)

**Fixed bugs:**

- When launching SoundSwitch, it always set as default the already default device [\#93](https://github.com/Belphemur/SoundSwitch/issues/93)
- Missing Beta mode in Settings [\#92](https://github.com/Belphemur/SoundSwitch/issues/92)
- Infinite notification popup [\#91](https://github.com/Belphemur/SoundSwitch/issues/91)

## [v3.9.8](https://github.com/Belphemur/SoundSwitch/tree/v3.9.8) (2016-04-29)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.7...v3.9.8)

**Implemented enhancements:**

- Use the Tooltip of the Systray instead of a Baloontip to display active device [\#90](https://github.com/Belphemur/SoundSwitch/issues/90)
- Show active device on hover Systray Icon [\#88](https://github.com/Belphemur/SoundSwitch/issues/88)
- Playback devices constantly go in the Disconnected section [\#87](https://github.com/Belphemur/SoundSwitch/issues/87)

## [v3.9.7](https://github.com/Belphemur/SoundSwitch/tree/v3.9.7) (2016-04-15)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.6...v3.9.7)

**Implemented enhancements:**

- Request: Cycle through connected devices by double clicking on the system tray icon [\#80](https://github.com/Belphemur/SoundSwitch/issues/80)
- Generate HTML for Changelog file and add it to the installer [\#79](https://github.com/Belphemur/SoundSwitch/issues/79)
- Providing 'help' for first time users at the time of installation [\#78](https://github.com/Belphemur/SoundSwitch/issues/78)

**Fixed bugs:**

- SoundSwitch 3.9.6 Cannot install visual c++ redist [\#84](https://github.com/Belphemur/SoundSwitch/issues/84)
- Pipe system broken: New instance of SoundSwitch doesn't close the previous one [\#81](https://github.com/Belphemur/SoundSwitch/issues/81)
- Switching already running applications [\#70](https://github.com/Belphemur/SoundSwitch/issues/70)

**Closed issues:**

- SoundSwitch crashes when logging in to a second user in Win10 [\#82](https://github.com/Belphemur/SoundSwitch/issues/82)
- Per-application switch [\#77](https://github.com/Belphemur/SoundSwitch/issues/77)

## [v3.9.6](https://github.com/Belphemur/SoundSwitch/tree/v3.9.6) (2016-03-10)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.5...v3.9.6)

**Implemented enhancements:**

- Add a "stealth" component to the Auto-Update [\#64](https://github.com/Belphemur/SoundSwitch/issues/64)

**Fixed bugs:**

- AutoUpdate doesn't restart correctly SoundSwitch [\#76](https://github.com/Belphemur/SoundSwitch/issues/76)
- Installer: Don't install if not Windows 7 SP1 or newer [\#75](https://github.com/Belphemur/SoundSwitch/issues/75)
- At windows startup, hotkeys don't work [\#72](https://github.com/Belphemur/SoundSwitch/issues/72)

**Closed issues:**

- Unable to installl dependencies, but Sound Switch would stil install. Crash at start [\#74](https://github.com/Belphemur/SoundSwitch/issues/74)

## [v3.9.5](https://github.com/Belphemur/SoundSwitch/tree/v3.9.5) (2016-02-28)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.4...v3.9.5)

## [v3.9.4](https://github.com/Belphemur/SoundSwitch/tree/v3.9.4) (2016-02-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.2...v3.9.4)

**Fixed bugs:**

- Empty Icon defined for an AudioDevice [\#71](https://github.com/Belphemur/SoundSwitch/issues/71)

**Closed issues:**

- Suggestion: Invoke command line to directly choose a source [\#69](https://github.com/Belphemur/SoundSwitch/issues/69)
- series of popups every restart [\#68](https://github.com/Belphemur/SoundSwitch/issues/68)

## [v3.9.2](https://github.com/Belphemur/SoundSwitch/tree/v3.9.2) (2016-01-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.1...v3.9.2)

**Implemented enhancements:**

- Make new instance of SoundSwitch close the previous one [\#66](https://github.com/Belphemur/SoundSwitch/issues/66)

**Closed issues:**

- Command line [\#61](https://github.com/Belphemur/SoundSwitch/issues/61)

## [v3.9.1](https://github.com/Belphemur/SoundSwitch/tree/v3.9.1) (2016-01-08)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.0...v3.9.1)

**Implemented enhancements:**

- Disabling completely the hotkeys [\#62](https://github.com/Belphemur/SoundSwitch/issues/62)

**Fixed bugs:**

- Sporadically hangs [\#43](https://github.com/Belphemur/SoundSwitch/issues/43)

## [v3.9.0](https://github.com/Belphemur/SoundSwitch/tree/v3.9.0) (2016-01-05)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.8.4...v3.9.0)

**Implemented enhancements:**

- Make update notification clearer for Windows 10 [\#59](https://github.com/Belphemur/SoundSwitch/issues/59)
- Personalized Notification Sound [\#52](https://github.com/Belphemur/SoundSwitch/issues/52)

**Closed issues:**

- Ability to switch default communications device as well. [\#60](https://github.com/Belphemur/SoundSwitch/issues/60)

## [v3.8.4](https://github.com/Belphemur/SoundSwitch/tree/v3.8.4) (2016-01-01)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.8.3...v3.8.4)

**Implemented enhancements:**

- Beta Channel [\#58](https://github.com/Belphemur/SoundSwitch/issues/58)

## [v3.8.3](https://github.com/Belphemur/SoundSwitch/tree/v3.8.3) (2015-12-31)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.8.2...v3.8.3)

**Fixed bugs:**

- SoundSwitch crash when disabling all Playback devices in the Control Panel [\#57](https://github.com/Belphemur/SoundSwitch/issues/57)
- Notification settings reset when Soundswitch closes when choosing "No Notification" \(3.8.2.25464\) [\#56](https://github.com/Belphemur/SoundSwitch/issues/56)

## [v3.8.2](https://github.com/Belphemur/SoundSwitch/tree/v3.8.2) (2015-12-30)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.8.1...v3.8.2)

**Fixed bugs:**

- Crash when using Windows Notification after SoundSwitch restart and switch device [\#55](https://github.com/Belphemur/SoundSwitch/issues/55)

## [v3.8.1](https://github.com/Belphemur/SoundSwitch/tree/v3.8.1) (2015-12-30)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.8.0...v3.8.1)

**Fixed bugs:**

- Soundswitch 3.7.0.23142 crashed when I tried to update it through the update button. [\#54](https://github.com/Belphemur/SoundSwitch/issues/54)
- App Freeze when using Sound Notification and Unplug device while sound playing. [\#53](https://github.com/Belphemur/SoundSwitch/issues/53)
- Crashes everytime I try to use it  [\#51](https://github.com/Belphemur/SoundSwitch/issues/51)

## [v3.8.0](https://github.com/Belphemur/SoundSwitch/tree/v3.8.0) (2015-12-29)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.7.0...v3.8.0)

**Implemented enhancements:**

- Personalize the Sound when switching device and play it on the new device. [\#49](https://github.com/Belphemur/SoundSwitch/issues/49)
- Create minidump in case of crash [\#48](https://github.com/Belphemur/SoundSwitch/issues/48)
- When an update is available, clicking on the notification launch the download [\#47](https://github.com/Belphemur/SoundSwitch/issues/47)

**Fixed bugs:**

- French language not loaded by SoundSwitch [\#50](https://github.com/Belphemur/SoundSwitch/issues/50)

## [v3.7.0](https://github.com/Belphemur/SoundSwitch/tree/v3.7.0) (2015-12-26)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.6.5...v3.7.0)

**Implemented enhancements:**

- Remove the use of Device Name in configuration for device ID [\#46](https://github.com/Belphemur/SoundSwitch/issues/46)

**Fixed bugs:**

- Version 3.6.5.13463 does not recognize playback devices, version 3.6.2.37344 does. [\#45](https://github.com/Belphemur/SoundSwitch/issues/45)
- By default SoundSwitch doesn't switch the Multimedia Device [\#44](https://github.com/Belphemur/SoundSwitch/issues/44)
- Crash and Restart when using Windows Remote Desktop. Problem when new device is connected. [\#41](https://github.com/Belphemur/SoundSwitch/issues/41)

## [v3.6.5](https://github.com/Belphemur/SoundSwitch/tree/v3.6.5) (2015-12-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.6.2...v3.6.5)

**Closed issues:**

- Default Audio device switching - programs are not [\#42](https://github.com/Belphemur/SoundSwitch/issues/42)

## [v3.6.2](https://github.com/Belphemur/SoundSwitch/tree/v3.6.2) (2015-12-12)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.6.1...v3.6.2)

**Implemented enhancements:**

- Add/Correct possibility to add only one key as hotkeys [\#37](https://github.com/Belphemur/SoundSwitch/issues/37)
- Toggle Option in the trayicon's menu [\#36](https://github.com/Belphemur/SoundSwitch/issues/36)

**Fixed bugs:**

- SoundSwitch Crashing when connecting a device [\#40](https://github.com/Belphemur/SoundSwitch/issues/40)
- Installer not detecting correctly VS 2015 Redist [\#39](https://github.com/Belphemur/SoundSwitch/issues/39)

**Closed issues:**

- Switching output bug in Chrome [\#38](https://github.com/Belphemur/SoundSwitch/issues/38)

## [v3.6.1](https://github.com/Belphemur/SoundSwitch/tree/v3.6.1) (2015-12-04)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.6.0...v3.6.1)

**Fixed bugs:**

- Installer doesn't detect DotNet 4.6.1 [\#35](https://github.com/Belphemur/SoundSwitch/issues/35)

**Merged pull requests:**

- Allow enable/disable display of notifications [\#33](https://github.com/Belphemur/SoundSwitch/pull/33) ([adamblackburn](https://github.com/adamblackburn))

## [v3.6.0](https://github.com/Belphemur/SoundSwitch/tree/v3.6.0) (2015-11-14)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.5.3...v3.6.0)

**Fixed bugs:**

- Display notification only once when default device is changed [\#32](https://github.com/Belphemur/SoundSwitch/issues/32)
- Soundswitch won't start. No crash report. [\#31](https://github.com/Belphemur/SoundSwitch/issues/31)

## [v3.5.3](https://github.com/Belphemur/SoundSwitch/tree/v3.5.3) (2015-11-12)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.5.2...v3.5.3)

## [v3.5.2](https://github.com/Belphemur/SoundSwitch/tree/v3.5.2) (2015-11-11)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.5.1...v3.5.2)

## [v3.5.1](https://github.com/Belphemur/SoundSwitch/tree/v3.5.1) (2015-09-18)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.5.0...v3.5.1)

**Implemented enhancements:**

- Update AudioEndPointLibrary [\#30](https://github.com/Belphemur/SoundSwitch/issues/30)

## [v3.5.0](https://github.com/Belphemur/SoundSwitch/tree/v3.5.0) (2015-09-14)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.4.2...v3.5.0)

**Implemented enhancements:**

- Implements improvement given by underlying library [\#29](https://github.com/Belphemur/SoundSwitch/issues/29)
- In case of crash. Zip the Log folder for easy reporting [\#24](https://github.com/Belphemur/SoundSwitch/issues/24)

## [v3.4.2](https://github.com/Belphemur/SoundSwitch/tree/v3.4.2) (2015-09-04)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.4.1...v3.4.2)

**Implemented enhancements:**

- Add localization [\#27](https://github.com/Belphemur/SoundSwitch/issues/27)
- Bigger Icons in Settings. Support non standard icons. [\#26](https://github.com/Belphemur/SoundSwitch/issues/26)

**Fixed bugs:**

- crash on Start: Recording Devices: Call IMMDeviceEnumerator::GetDefaultAudioEndpoint\(...\)  [\#25](https://github.com/Belphemur/SoundSwitch/issues/25)

## [v3.4.1](https://github.com/Belphemur/SoundSwitch/tree/v3.4.1) (2015-09-02)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.4.0...v3.4.1)

**Fixed bugs:**

- crashes on start: IndexOutOfRangeException [\#23](https://github.com/Belphemur/SoundSwitch/issues/23)

## [v3.4.0](https://github.com/Belphemur/SoundSwitch/tree/v3.4.0) (2015-09-02)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.3.1...v3.4.0)

**Implemented enhancements:**

- Group devices by state in Settings [\#22](https://github.com/Belphemur/SoundSwitch/issues/22)
- Switch Recording devices [\#19](https://github.com/Belphemur/SoundSwitch/issues/19)

**Closed issues:**

- Error: Index was outside the bounds of the array [\#20](https://github.com/Belphemur/SoundSwitch/issues/20)

## [v3.3.1](https://github.com/Belphemur/SoundSwitch/tree/v3.3.1) (2015-08-29)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.3.0...v3.3.1)

**Fixed bugs:**

- Settings not saving [\#21](https://github.com/Belphemur/SoundSwitch/issues/21)

## [v3.3.0](https://github.com/Belphemur/SoundSwitch/tree/v3.3.0) (2015-08-27)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.2.2...v3.3.0)

**Implemented enhancements:**

- Access the Windows Sound Mixer [\#18](https://github.com/Belphemur/SoundSwitch/issues/18)
- Update Icons to higher quality [\#17](https://github.com/Belphemur/SoundSwitch/issues/17)
- Add devices icons in the Settings Form [\#16](https://github.com/Belphemur/SoundSwitch/issues/16)
- Use AudioEndPointLibrary to manage audio devices [\#15](https://github.com/Belphemur/SoundSwitch/issues/15)
- Idea: Add function to switch "Default communication device" [\#14](https://github.com/Belphemur/SoundSwitch/issues/14)
- Auto-Updater [\#12](https://github.com/Belphemur/SoundSwitch/issues/12)
- Sign the application and installer [\#7](https://github.com/Belphemur/SoundSwitch/issues/7)

## [v3.2.2](https://github.com/Belphemur/SoundSwitch/tree/v3.2.2) (2015-08-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.2.1...v3.2.2)

**Implemented enhancements:**

- Add a logger to easily debug release version [\#9](https://github.com/Belphemur/SoundSwitch/issues/9)

**Fixed bugs:**

- Installer doesn't detect .NET 4.6 Preview [\#11](https://github.com/Belphemur/SoundSwitch/issues/11)
- SoundSwitch Settings menu cause appcrash [\#10](https://github.com/Belphemur/SoundSwitch/issues/10)
- SoundSwitch appears to crash immediately after launch [\#5](https://github.com/Belphemur/SoundSwitch/issues/5)

**Closed issues:**

- Application crashing at launch [\#8](https://github.com/Belphemur/SoundSwitch/issues/8)

## [v3.2.1](https://github.com/Belphemur/SoundSwitch/tree/v3.2.1) (2015-08-23)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.2.0...v3.2.1)

## [v3.2.0](https://github.com/Belphemur/SoundSwitch/tree/v3.2.0) (2015-08-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.1.2...v3.2.0)

**Implemented enhancements:**

- Make the uinstaller remove the startup registry key [\#4](https://github.com/Belphemur/SoundSwitch/issues/4)
- Add Changelog in installer [\#3](https://github.com/Belphemur/SoundSwitch/issues/3)
- Implement a better AutoStart feature [\#2](https://github.com/Belphemur/SoundSwitch/issues/2)

**Fixed bugs:**

- Program prevents system logout/shutdown [\#1](https://github.com/Belphemur/SoundSwitch/issues/1)

## [v3.1.2](https://github.com/Belphemur/SoundSwitch/tree/v3.1.2) (2015-08-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.1.1...v3.1.2)

## [v3.1.1](https://github.com/Belphemur/SoundSwitch/tree/v3.1.1) (2015-08-20)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.1.0...v3.1.1)

## [v3.1.0](https://github.com/Belphemur/SoundSwitch/tree/v3.1.0) (2015-08-20)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.0.1...v3.1.0)

## [v3.0.1](https://github.com/Belphemur/SoundSwitch/tree/v3.0.1) (2015-08-19)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.0.0...v3.0.1)

## [v3.0.0](https://github.com/Belphemur/SoundSwitch/tree/v3.0.0) (2015-08-19)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v2.5.0...v3.0.0)

## [v2.5.0](https://github.com/Belphemur/SoundSwitch/tree/v2.5.0) (2015-08-17)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/1c44822dcabe2ace0323550db929762f62a6a710...v2.5.0)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
