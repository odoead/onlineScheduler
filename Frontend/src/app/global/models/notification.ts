export interface Notification_{
      id:number 
      recieverId:string
      service:string
      title:string 
      description :string
      notificationKeyValues: { [key: string]: string };
}
