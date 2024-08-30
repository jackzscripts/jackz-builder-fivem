import NativeUI, { ItemsCollection, ListMenu, Menu, UIMenuItem } from "../../NativeUI/dist/nativeui/NativeUi.js";
import { MENU_ORIGIN } from '../consts.js'

import EventEmitter from "events"
import TypedEmitter from "typed-emitter"
import Point from '../../NativeUI/dist/nativeui/utils/Point';

export class BuilderMenu extends Menu {
    #description: string

    constructor( title: string, description?: string ) {
        super( "", title, MENU_ORIGIN )
        this.SetNoBannerType()
        this.#description = description ?? ""
    }

    get Title() { return super.SubTitle }
    set Title( title: string ) { super.SubTitle = title }
    
    get Description() { return this.#description }
    set Description( value: string ) { this.#description = value }

    AddSubMenu( child: Menu | BuilderMenu | BuilderListMenu): void {
        const description = child instanceof BuilderMenu ? child.Description : ''
        super.AddSubMenu( child, new UIMenuItem( child.SubTitle, description ) )
    }

    // AddItem(item: BuilderItem) {
    //     super.AddItem(item.Item);
    // }
}

export class BuilderListMenu extends ListMenu {
    #description: string

    constructor( title: string, description: string, items: ItemsCollection) {
        super( "", title, items, MENU_ORIGIN )
        this.#description = description ?? ""
        this.SetNoBannerType()
    }

    get Title() { return super.SubTitle }
    set Title( title: string ) { super.SubTitle = title }

    get Description() { return this.#description }
    set Description( value: string ) { this.#description = value }

}