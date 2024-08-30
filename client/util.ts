
export function Delay( ms: number ) { return new Promise( res => setTimeout( res, ms ) ) }

/**
 * Loads a model
 * @param modelId model name
 * @returns Hash of model
 */
export async function LoadModel( modelId: string ): Promise<number> {
    const hash = GetHashKey( modelId )
    if ( !IsModelValid( hash ) ) throw new Error( "Model is invalid: " + hash )

    // Load the model, with cancellation support
    global.RequestModel( hash )
    while ( !HasModelLoaded( hash ) ) {
        await Delay( 200 )
    }
    return hash
}
export async function PromptText( maxLength: number = 30, defaultValue = ""): Promise<string> {
    DisplayOnscreenKeyboard( 1, "FMMC_MPM_NA", "", defaultValue, "", "", "", maxLength );
    let state = UpdateOnscreenKeyboard()
    while ( state === 0 ) {
        state = UpdateOnscreenKeyboard();
        await Delay(100)
    }
    if ( state == 1 ) {
        const result = GetOnscreenKeyboardResult();
        if ( result != null ) {
            return result
        } else {
            throw new Error("No result")
        }
    } else if ( state == 2 ) {
        throw new Error( "Cancelled" )
    } else if ( state == -1 ) {
        throw new Error( "Not active" )
    } else {
        throw new Error("Not possible error")
    }
}
export function RemoveAllAttachments( handle: number ) {
    recurseRemoveAttachments( handle, GetGamePool("CPed") );
    recurseRemoveAttachments( handle, GetGamePool("CObject") );
    recurseRemoveAttachments( handle, GetGamePool("CVehicle") );
}

function recurseRemoveAttachments( parent: number, entities: number[] ) {
    for(const entity of entities) {
        if ( parent === entity ) continue;
        for ( const subEntity of entities ) {
            if ( subEntity != entity && subEntity != parent && IsEntityAttachedToEntity(subEntity, entity) ) {
                DeleteEntity(subEntity)
            }
        }
        if ( IsEntityAttachedToEntity( entity, parent ) ) {
            DeleteEntity(entity)
        }
    }
}