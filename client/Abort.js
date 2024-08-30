export class AbortSignal {
  constructor() {
    this.aborted = false;
    this.reason = undefined;
  }
  toString() {
    return "[object AbortSignal]";
  }
  get [Symbol.toStringTag]() {
    return "AbortSignal";
  }
  throwIfAborted() {
    if (this.aborted) {
      throw this.reason;
    }
  }
}
export class AbortController {
  constructor() {
    this.signal = new AbortSignal();
  }
  abort(reason) {
    if (this.signal.aborted) return;

    this.signal.aborted = true;

    if (reason) this.signal.reason = reason;
    else this.signal.reason = new Error("AbortError");

    this.signal.dispatchEvent("abort");
  }
  toString() {
    return "[object AbortController]";
  }
  get [Symbol.toStringTag]() {
    return "AbortController";
  }
}
