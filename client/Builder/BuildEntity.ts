import { Vector3 } from "../../shared/types.js";

export enum EntityType {
    Entity,
    Object,
    Vehicle,
    Ped
}

export class BuildEntity {
    #offset: Vector3 = [0, 0, 0];
    #handle: number;

    #type: EntityType = EntityType.Entity

    constructor( entityHandle: number ) {
        this.#handle = entityHandle;
        if ( IsEntityAnObject( entityHandle ) ) {
            this.#type = EntityType.Object
        } else if ( IsEntityAPed( entityHandle ) ) {
            this.#type = EntityType.Ped
        } else if ( IsEntityAVehicle( entityHandle ) ) {
            this.#type = EntityType.Vehicle
        }
    }

    public get Type() { return this.#type }

    public get Rotation(): Vector3 { 
        return GetEntityRotation(this.#handle, 2) as Vector3
    }
    public set Rotation( value: Vector3 ) {
        SetEntityRotation(this.#handle, value[0], value[1], value[2], 2, false)
    }

    public get Offset(): Vector3 {
        return this.#offset;
    }
    public set Offset( value: Vector3 ) {
        this.#offset = value;
        this.Attach()
    }

    Attach() {

    }
}
