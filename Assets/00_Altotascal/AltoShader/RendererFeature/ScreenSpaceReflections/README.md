
# URP 向け Screen Space Reflections

![](../../../../../Docs/img/ssr-1.jpg)

- 本リポジトリの独自シェーダと合わせて使うことで Screen Space Reflections を描画できる Renderer Feature です
- URP（Unity 6 以降の RenderGraph 系）で動作します（Compatibility Mode には対応していません）

## セットアップ手順

- このフォルダのコードをプロジェクトに放り込めば動くはず（たぶん）
- Renderer の設定アセットのインスペクタで Add Renderer Feature → SSR_Renderer Feature を追加
- シーンに置いた Volume Profile で Add Override → Alto > Screen Space Reflections を追加
    - 設定で Intensity を 0 より大きくして有効化
- AltoShader / SakanaShader / StylizedWater の Material で SSR Reflectivity を 0 より大きくしたものが反射の描画対象になります
    - （内部的には、シェーダの DepthNormals パスで描かれたノーマルの alpha チャンネルが 0 より大きいフラグメントが対象になる仕様）
