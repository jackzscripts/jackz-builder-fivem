import * as NativeUI from "../NativeUI/dist/nativeui/NativeUi.js";
import Build, { ActiveBuild } from "./Builder/Build.js";
import { RELEASE_CHANNEL, RESOURCE_VERSION } from './consts.js'
import { BuilderMenu } from "./MenuAPI/BuilderMenu.js";
import { clearPreview, previewVehicle } from "./preview.js";
import UIMenuListItem from '../NativeUI/dist/nativeui/items/UIMenuListItem';
import { PROPS_CURATED } from "./data/props.js";

export function openMenu() {
    const menu = new BuilderMenu( "Main Menu" );
    menu.AddSubMenu(createMetaMenu())
    menu.AddSubMenu(createBuilderMenu())
    menu.Visible = true
}

function createBuilderMenu() {
    const menu = new BuilderMenu( "Builder", "" )
    updateBuilderMenu( menu )
    return menu
}
function updateBuilderMenu( menu: BuilderMenu ) {
    menu.Clear()
    if ( ActiveBuild.get() ) {
        const spawnerMenu = new BuilderMenu( "Spawner", "Spawn new entities" )
        const entityEditMenu = new BuilderMenu( "Entities", "Edit spawned entities" )

        menu.AddSubMenu( createSpawnerMenu() )
        menu.AddSubMenu( createEntityEditMenu() )

        const testPreviewStart = new NativeUI.UIMenuItem( "Start Preview" )
        const testPreviewStop = new NativeUI.UIMenuItem( "Stop Preview" )
        testPreviewStart.On( "select", async () => {
            previewVehicle( "t20" )
        } )
        testPreviewStop.On( "select", () => {
            clearPreview()
        } )
        menu.AddItem( testPreviewStart )
        menu.AddItem( testPreviewStop )
    } else {
        const startStructureItem = new NativeUI.UIMenuItem( "Start new structure", "Creates a new structure" )
        startStructureItem.On( "select", async () => {
            const build = await Build.StartNewStructure()
            updateBuilderMenu(menu)
        } )
        menu.AddItem( startStructureItem )
    }
    return menu
}

function createMetaMenu() {
    const menu = new BuilderMenu( "Meta", "Information relating to resource ");

    const versionItem = new NativeUI.UIMenuItem( "Version", "The current version of the resource" )
    versionItem.SetRightLabel( RESOURCE_VERSION )
    menu.AddItem( versionItem )

    const channelItem = new NativeUI.UIMenuItem( "Release Channel", "The release channel of the item, such as Development or Release" )
    channelItem.SetRightLabel( RELEASE_CHANNEL )
    menu.AddItem( channelItem )

    return menu
}

function createSpawnerMenu() {
    const menu = new BuilderMenu( "Spawner", "Spawn new entities" )
    menu.AddSubMenu(createPropSpawner())
    return menu
}

function createPropSpawner() {
    const menu = new BuilderMenu( "Props", "Spawn new props" )

    setupLoader()
    menu.On( "open", async () => {
        const props = await import( "./data/props.js" )
        menu.AddItem( new UIMenuListItem( "Curated Props", "", new NativeUI.ItemsCollection( props.PROPS_CURATED ) ) )

        menu.RemoveItemAtIndex(0)
    } )
    menu.On( "close", () => {
        menu.Clear()
        setupLoader()
    })
    
    function setupLoader() {
        const loadingItem = new NativeUI.UIMenuItem( "Loading props list" )
        loadingItem.Enabled = false
        menu.AddItem( loadingItem )
    }

    return menu
}


function createEntityEditMenu() {
    const menu = new BuilderMenu( "Entities", "Edit spawned entities" )
    return menu
}