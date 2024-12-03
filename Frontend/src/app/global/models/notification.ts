export interface Notification_{
      Id:number 
      RecieverId:string
      Service:string
      Title:string 
      Description :string
      notificationKeyValues: { [key: string]: string };
}
