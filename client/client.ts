import { openMenu } from './menu.js'
import { clearPreview } from './preview.js'

on( 'onResourceStart', ( resName: string ) => {
  if ( resName === GetCurrentResourceName() ) {
    console.log( "Started", new Date() )
    
    openMenu()
  }
} )

on( 'onResourceStop', ( resName: string ) => {
  if ( resName === GetCurrentResourceName() ) {
    clearPreview()
  }
} )

RegisterCommand("builder", openMenu, true)