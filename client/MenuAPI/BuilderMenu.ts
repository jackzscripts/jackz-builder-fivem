import NativeUI, { Menu, UIMenuItem } from "../../NativeUI/dist/nativeui/NativeUi.js";
import { MENU_ORIGIN } from '../consts.js'

import EventEmitter from "events"
import TypedEmitter from "typed-emitter"

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

    AddSubMenu( child: Menu | BuilderMenu ): void {
        const description = child instanceof BuilderMenu ? child.Description : ''
        super.AddSubMenu( child, new UIMenuItem( child.SubTitle, description ) )
    }

    // AddItem(item: BuilderItem) {
    //     super.AddItem(item.Item);
    // }
}

// export class BaseBuilderItem extends UIMenuItem  {
//     get Parent(): NativeUI {
//         return this.#parent;
//     }

//     set Parent(value: NativeUI) {
//         this.#parent = value;
//     }
//     #parent: Menu
// }
// export class BuilderItem extends (EventEmitter as new () => TypedEmitter<ItemEvents>){
//     get Item(): BaseBuilderItem {
//         return this.#item;
//     }

//     set Item(value: UIMenuItem) {
//         this.#item = value;
//     }
//     #item: UIMenuItem

//     constructor(label: string, description?: string) {
//         super();
//         this.#item = new UIMenuItem(label, description)
//     }


// }