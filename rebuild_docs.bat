@echo off
if exist ualbion.wiki goto exists
git clone https://github.com/csinkers/ualbion.wiki.git
:exists
pushd ualbion.wiki
git stash && git pull && git stash pop
:: todo
