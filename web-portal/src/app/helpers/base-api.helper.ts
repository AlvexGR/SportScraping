import { ApiResult } from "../models/api-result.model";
import { HttpHeaders, HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Utility } from './utility.helper';

@Injectable({
  providedIn: "root"
})
export class BaseApiHelper {
  //static baseUrl = "https://localhost:44326/api"; // Local
  //static baseUrl = "http://192.168.1.10:1503/api"; // Home
  //static baseUrl = "http://192.168.101.117:1503/api" // IMT
  static baseUrl = "http://localhost:57775/api" // server

  constructor(private _http: HttpClient) {}

  /**
   * Create approriate header for all requests
   * @param accessToken for Authorization header
   */
  private createHeader(): HttpHeaders {
    return new HttpHeaders({
      "Content-Type": "application/json",
      Accept: "application/json"
    });
  }

  /**
   * Return a promise to await, act like waiting for server to response
   * @param waitTime time to wait in milliseconds
   */
  public imitateResponseBehavior(waitTime: number): Promise<void> {
    return Utility.delay(waitTime);
  }

  /**
   * Generic get method
   * @param url api url
   */
  public async get<T>(url: string): Promise<T> {
    const headers = this.createHeader();
    let result: T;
    try {
      const response = await this._http
        .get<ApiResult<T>>(url, {
          headers,
          observe: "response"
        })
        .toPromise();

      if (!response.body.succeed) {
        throw new Error(response.body.error);
      }

      result = response.body.result;
    } catch (err) {
      console.log(err);
      throw new Error(err);
    }
    return result;
  }

  /**
   * Download file
   * @param url api url
   */
  public async downloadFile(url: string): Promise<Blob> {
    let result: Blob;
    try {
      const response = await this._http
        .get(url, {
          observe: "response",
          responseType: "blob",
        })
        .toPromise();
      result = response.body;
    } catch (err) {
      console.log(err);
      throw new Error(err);
    }
    return result;
  }
}
