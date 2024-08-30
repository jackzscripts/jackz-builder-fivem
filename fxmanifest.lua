fx_version 'cerulean'
name 'FiveM TypeScript Boilerplate'
author 'Project Error'
game 'gta5'

server_script 'dist/server/**/*.js'
client_script {
    "@ScaleformUI_Lua/ScaleformUI.lua",
    'dist/client/**/*.js'
}

