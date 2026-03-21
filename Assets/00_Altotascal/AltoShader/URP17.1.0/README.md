
# URP 17.1.0 向けカスタムシェーダ

- このフォルダ配下のシェーダは URP 標準の SimpleLit シェーダのコードをベースに制作したもの
- URP 標準の `Lighting.hlsl` は利用しているが、ライティングのコアロジックは
  中身を参照しつつ手を加えられるようにするため、`_SharedLogic/URPBridge-*` に
  （関数名先頭に Alto_ を付けて）コピーした上で使用している
- URP 更新時に大元のロジックに大きな変更があった場合は、
  この `URPBridge-*` 系統を追従させてマイグレーションする
- ただし `AltoShader-ForwardPass.hlsl` などは `UniversalFragmentBlinnPhong()`
  あたりに多く手を入れて独自実装しているため、マイグレーション時はこちらもケアする必要がある

----

- `_SharedLogic/CustomEffect-*` は独自エフェクトのアルゴリズムを個別に切り出したもの。
  ここは基本型のみを参照するので URP バージョンに依存するコードを含まない想定
