
kyubunsさんが作成されたprefab_in_prefabを、自分用に
修正したバージョンです。

ご使用は自己責任にてお願いいたします。


修正内容
　・[PrefabInPrefab.cs]内のVirtualPrefab作成処理を[VirtualPrefabCreater.cs]
　　へ移動。

　・循環参照のチェックを[PrefabInPrefabEditor.cs]へ移動

　・VirtualPrefabの作成先をPrefabInPrefab_VirtualPrefabへ固定

　・コーディング規約を自分用に修正

　・[VirtualPrefab.cs]内で毎フレームTransformの更新をしていて、オブジェクト数が
　　多い場合に重かったので、位置の変化時のみ処理するように条件追加

================

prefab_in_prefab
================

What is this
* prefab in prefab for unity4.5
*
* Movie
*   https://www.youtube.com/watch?v=wSeHsAQZFb0
* Japanese
*   http://qiita.com/kyubuns/items/5741e5281f4bb8de656c

How to use
* cp prefab_in_prefab/Assets/PrefabInPrefab your_project/Assets/

Features
* 'Prefab in Prefab' in play/edit mode
* 'Prefab in Prefab in Prefab' in play/edit mode
* 'Prefab in Prefab in Prefab in Prefab' in play/edit mode
* 'Prefab in Prefab in Prefab in Prefab in ...' in play/edit mode

License
* The MIT License - Copyright (c) 2014 kyubuns

