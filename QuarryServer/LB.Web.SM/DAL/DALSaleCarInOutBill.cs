﻿using LB.Web.Base.Factory;
using LB.Web.Base.Helper;
using LB.Web.Contants.DBType;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace LB.Web.SM.DAL
{
    public class DALSaleCarInOutBill
    {
        //判断该车辆是否已出磅
        public DataTable ExistsNotOut(FactoryArgs args, t_BigID CarID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CarID", CarID));
            string strSQL = @"
    select BillDate
    from dbo.SaleCarInBill
    where CarID=@CarID and isnull(IsCancel,0)=0 and 
        SaleCarInBillID not in (select SaleCarInBillID from dbo.SaleCarOutBill)
";
            return DBHelper.ExecuteQuery(args, strSQL, parms);
        }

        public DataTable GetMaxOutBillCode(FactoryArgs args,string strBillFont)
        {

            string strSQL = @"
select top 1 SaleCarOutBillCode
from dbo.SaleCarOutBill
where SaleCarOutBillCode like '" + strBillFont + @"%'
order by SaleCarOutBillCode desc
";
            return DBHelper.ExecuteQuery(args, strSQL);
        }

        public void GetInBillSpecialCode(FactoryArgs args,t_BigID CustomerID,t_BigID ItemID,out t_String BillCode)
        {
            BillCode = new t_String();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CustomerID", CustomerID));
            parms.Add(new LBDbParameter("ItemID", ItemID));
            parms.Add(new LBDbParameter("BillCode", BillCode,true));

            string strSQL = @"
EXEC dbo.SaleCarInBill_SprcialBillCode @CustomerID,@ItemID,@BillCode output
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            BillCode.Value = parms["BillCode"].Value.ToString();
        }

        public void GetOutBillSpecialCode(FactoryArgs args, t_BigID CustomerID, t_BigID ItemID, out t_String BillCode)
        {
            BillCode = new t_String();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CustomerID", CustomerID));
            parms.Add(new LBDbParameter("ItemID", ItemID));
            parms.Add(new LBDbParameter("BillCode", BillCode, true));

            string strSQL = @"
EXEC dbo.SaleCarOutBill_SprcialBillCode @CustomerID,@ItemID,@BillCode output
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            BillCode.Value = parms["BillCode"].Value.ToString();
        }

        public DataTable GetMaxInBillCode(FactoryArgs args, string strBillFont)
        {

            string strSQL = @"
select top 1 SaleCarInBillCode
from dbo.SaleCarInBill
where SaleCarInBillCode like '" + strBillFont + @"%'
order by SaleCarInBillCode desc
";
            return DBHelper.ExecuteQuery(args, strSQL);
        }

        public void InsertInBill(FactoryArgs args, out t_BigID SaleCarInBillID, t_String SaleCarInBillCode, t_BigID CarID,
            t_BigID ItemID, t_DTSmall BillDate, t_ID ReceiveType, t_ID CalculateType, t_Float CarTare, t_BigID CustomerID, t_String Description,
            t_ID SaleBillType,t_BigID SaleCarInBillIDFromClient, t_String CreateBy)
        {
            SaleCarInBillID = new t_BigID();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID, true));
            parms.Add(new LBDbParameter("SaleCarInBillCode", SaleCarInBillCode));
            parms.Add(new LBDbParameter("CarID", CarID));
            parms.Add(new LBDbParameter("ItemID", ItemID));
            parms.Add(new LBDbParameter("BillDate", BillDate));
            parms.Add(new LBDbParameter("ReceiveType", ReceiveType));
            parms.Add(new LBDbParameter("CalculateType", CalculateType));
            parms.Add(new LBDbParameter("CarTare", CarTare));
            parms.Add(new LBDbParameter("CustomerID", CustomerID));
            parms.Add(new LBDbParameter("Description", Description));
            parms.Add(new LBDbParameter("SaleBillType", SaleBillType));
            parms.Add(new LBDbParameter("SaleCarInBillIDFromClient", SaleCarInBillIDFromClient));
            parms.Add(new LBDbParameter("CreateBy", CreateBy));

            string strSQL = @"
insert into dbo.SaleCarInBill(  SaleCarInBillCode, CarID,PrintCount,
            ItemID, BillDate, ReceiveType, BillStatus, CalculateType, CarTare, CustomerID,Description,
            IsCancel, CreateBy, CreateTime,CancelByDate,SaleBillType,SaleCarInBillIDFromClient)
values( @SaleCarInBillCode, @CarID,0,
        @ItemID, @BillDate, @ReceiveType, 1, @CalculateType, @CarTare, @CustomerID,@Description,
        0,@CreateBy,@BillDate,null,@SaleBillType,@SaleCarInBillIDFromClient)

set @SaleCarInBillID = @@identity

update dbo.DbCar
set SortLevel = isnull(SortLevel,0)+1
where CarID = @CarID

update dbo.DbCustomer
set SortLevel = isnull(SortLevel,0)+1
where CustomerID = @CustomerID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            SaleCarInBillID.Value = Convert.ToInt64(parms["SaleCarInBillID"].Value);
        }

        public DataTable GetCarNotOutBill(FactoryArgs args, t_BigID CarID,t_String CarNum)
        {
            CarID.IsNullToZero();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CarID", CarID));
            parms.Add(new LBDbParameter("CarNum", CarNum));

            string strSQL = @"

select b.*
from dbo.SaleCarInBill b
    inner join DbCar c on
        c.CarID = b.CarID
where b.CarID = @CarID and
    b.SaleCarInBillID not in (
    select SaleCarInBillID
    from dbo.SaleCarOutBill
    ) and isnull(BillStatus,0)=1 and isnull(IsCancel,0)=0
order by b.BillDate desc
";
            if (CarID.Value == 0&& CarNum.Value!="")
            {
                strSQL = @"
select b.*
from dbo.SaleCarInBill b
    inner join DbCar c on
        c.CarID = b.CarID
where c.CarNum = rtrim(@CarNum) and
    b.SaleCarInBillID not in (
    select SaleCarInBillID
    from dbo.SaleCarOutBill
    ) and isnull(BillStatus,0)=1 and isnull(IsCancel,0)=0
order by b.BillDate desc
";
            }
            return DBHelper.ExecuteQuery(args, strSQL, parms);
        }

        public DataTable GetCustomer(FactoryArgs args, t_BigID CustomerID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CustomerID", CustomerID));

            string strSQL = @"
select *
from dbo.DbCustomer
where CustomerID = @CustomerID
";
            return DBHelper.ExecuteQuery(args, strSQL, parms);
        }

        public DataTable GetSaleCarInBill(FactoryArgs args, t_BigID SaleCarInBillID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));

            string strSQL = @"
select *
from dbo.SaleCarInBill b
    inner join dbo.DbCar c on
        c.CarID = b.CarID
where SaleCarInBillID = @SaleCarInBillID
";
            return DBHelper.ExecuteQuery(args, strSQL, parms);
        }

        public DataTable GetCarOutBillByInBillID(FactoryArgs args, t_BigID SaleCarInBillID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));

            string strSQL = @"
select *
from dbo.SaleCarOutBill
where SaleCarInBillID = @SaleCarInBillID
";
            return DBHelper.ExecuteQuery(args, strSQL, parms);
        }

        public void InsertOutBill(FactoryArgs args, out t_BigID SaleCarOutBillID,t_String SaleCarOutBillCode, t_BigID SaleCarInBillID, t_BigID CarID, t_DTSmall BillDate,
            t_Decimal TotalWeight, t_Decimal SuttleWeight, t_Decimal Price, t_Decimal Amount, t_ID ReceiveType, t_ID CalculateType, t_String Description,
            t_String CreateBy,t_BigID SaleCarOutBillIDFromClient, t_Decimal MaterialPrice, t_Decimal FarePrice,
                t_Decimal TaxPrice, t_Decimal BrokerPrice)
        {
            SaleCarOutBillID = new t_BigID();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarOutBillID", SaleCarOutBillID, true));
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("SaleCarOutBillCode", SaleCarOutBillCode));
            parms.Add(new LBDbParameter("BillDate", BillDate));
            parms.Add(new LBDbParameter("CarID", CarID));
            parms.Add(new LBDbParameter("TotalWeight", TotalWeight));
            parms.Add(new LBDbParameter("SuttleWeight", SuttleWeight));
            parms.Add(new LBDbParameter("Price", Price));
            parms.Add(new LBDbParameter("Amount", Amount));
            parms.Add(new LBDbParameter("Description", Description));
            parms.Add(new LBDbParameter("ReceiveType", ReceiveType));
            parms.Add(new LBDbParameter("CalculateType", CalculateType));
            parms.Add(new LBDbParameter("CreateBy", CreateBy));
            parms.Add(new LBDbParameter("SaleCarOutBillIDFromClient", SaleCarOutBillIDFromClient));
            parms.Add(new LBDbParameter("MaterialPrice", MaterialPrice));
            parms.Add(new LBDbParameter("FarePrice", FarePrice));
            parms.Add(new LBDbParameter("TaxPrice", TaxPrice));
            parms.Add(new LBDbParameter("BrokerPrice", BrokerPrice));

            string strSQL = @"
insert into dbo.SaleCarOutBill(  SaleCarInBillID,SaleCarOutBillCode, BillDate, CarID,TotalWeight,
            SuttleWeight, Price, Amount, Description,CreateBy, CreateTime,SaleCarOutBillIDFromClient,
            MaterialPrice,FarePrice,TaxPrice,BrokerPrice,ChangedBy,ChangeTime)
values( @SaleCarInBillID,@SaleCarOutBillCode, @BillDate, @CarID, @TotalWeight,
        @SuttleWeight, @Price, @Amount, @Description,@CreateBy,@BillDate,@SaleCarOutBillIDFromClient,
        @MaterialPrice,@FarePrice,@TaxPrice,@BrokerPrice,@CreateBy,getdate())

set @SaleCarOutBillID = @@identity

update dbo.SaleCarInBill
set ReceiveType = @ReceiveType,
    CalculateType = @CalculateType
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            SaleCarOutBillID.Value = Convert.ToInt64(parms["SaleCarOutBillID"].Value);
        }

        public void UpdateOutBillAmount(FactoryArgs args, t_BigID SaleCarInBillID,
            t_Decimal Price,t_Decimal Amount)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("Price", Price));
            parms.Add(new LBDbParameter("Amount", Amount));
            parms.Add(new LBDbParameter("ChangedBy", new t_String(args.LoginName)));
            string strSQL = @"
update  dbo.SaleCarOutBill
set Price = @Price,
    Amount = @Amount,
    ChangedBy = @ChangedBy,
    ChangeTime=getdate()
where SaleCarInBillID = @SaleCarInBillID";

        }

        public void UpdateOutBillCustomer(FactoryArgs args, t_BigID SaleCarInBillID,
            t_BigID CustomerID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("CustomerID", CustomerID));
            parms.Add(new LBDbParameter("ChangedBy", new t_String(args.LoginName)));
            string strSQL = @"
update  dbo.SaleCarOutBill
set CustomerID = @CustomerID,
    ChangedBy = @ChangedBy,
    ChangeTime=getdate()
where SaleCarInBillID = @SaleCarInBillID";

        }

        public DataTable GetGetSaleCarInOutBill(FactoryArgs args, t_BigID SaleCarInBillID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));

            string strSQL = @"
select SaleCarOutBillID,i.BillDate as BillDateIn,o.BillDate as BillDateOut, o.CreateBy as OutCreateBy ,c.CarNum,i.*,o.*,
        b.K3ItemCode,s.K3CustomerCode,s.CustomerName
from dbo.SaleCarInBill i
    left outer join SaleCarOutBill o on 
        i.SaleCarInBillID = o.SaleCarInBillID
    left outer join DbCar c on 
        c.CarID = i.CarID
    left outer join dbo.DBItemBase b on
        b.ItemID = i.ItemID
    inner join dbo.DBCustomer s on
        s.CustomerID = i.CustomerID
where i.SaleCarInBillID = @SaleCarInBillID
";
            return DBHelper.ExecuteQuery(args, strSQL, parms);
        }

        public void Approve(FactoryArgs args, t_BigID SaleCarInBillID, t_DTSmall ApproveTime)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("ApproveTime", ApproveTime));
            parms.Add(new LBDbParameter("ApproveBy", new t_String(args.LoginName)));
            string strSQL = @"
                update dbo.SaleCarInBill
                set BillStatus=2,
                    ApproveTime = @ApproveTime,
                    ApproveBy=@ApproveBy
                where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void UnApprove(FactoryArgs args, t_BigID SaleCarInBillID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            string strSQL = @"
                update dbo.SaleCarInBill
                set BillStatus=1,ApproveTime=null,ApproveBy=null
                where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void Cancel(FactoryArgs args, t_BigID SaleCarInBillID, t_String CancelDesc)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("CancelBy", new t_String(args.LoginName)));
            parms.Add(new LBDbParameter("CancelDesc", CancelDesc));
            string strSQL = @"
                update dbo.SaleCarInBill
                set IsCancel=1,
                    CancelBy = @CancelBy,
                    CancelTime = getdate(),
                    CancelDesc = @CancelDesc
                where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void UnCancel(FactoryArgs args, t_BigID SaleCarInBillID, t_DTSmall CancelByDate)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("CancelByDate", CancelByDate));
            string strSQL = @"
                update dbo.SaleCarInBill
                set IsCancel=0,
                    CancelBy = '',
                    CancelTime = null,
                    CancelByDate= @CancelByDate
                where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void UpdateCustomerSalesAmount(FactoryArgs args,t_BigID CustomerID,t_Decimal SalesReceivedAmount)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CustomerID", CustomerID));
            parms.Add(new LBDbParameter("SalesReceivedAmount", SalesReceivedAmount));
            string strSQL = @"
update dbo.DbCustomer
set SalesReceivedAmount = isnull(SalesReceivedAmount,0)+isnull(@SalesReceivedAmount,0)
where CustomerID = @CustomerID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }
        
        public void ReadCarID(FactoryArgs args,t_String CarNum,out t_BigID CarID)
        {
            CarID = new t_BigID();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CarNum", CarNum));
            parms.Add(new LBDbParameter("CarID", CarID,true));
            string strSQL = @"
select @CarID = CarID
from dbo.DbCar
where rtrim(CarNum) = rtrim(@CarNum)
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            CarID.SetValueWithObject(parms["CarID"].Value);
        }

        public void ReadItemID(FactoryArgs args, t_String ItemName, out t_BigID ItemID)
        {
            ItemID = new t_BigID();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("ItemName", ItemName));
            parms.Add(new LBDbParameter("ItemID", ItemID, true));
            string strSQL = @"
select @ItemID = ItemID
from dbo.DbItem
where rtrim(ItemName) = rtrim(@ItemName)
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            ItemID.SetValueWithObject(parms["ItemID"].Value);
        }

        public void ReadCustomerID(FactoryArgs args, t_String CustomerName, out t_BigID CustomerID)
        {
            CustomerID = new t_BigID();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CustomerName", CustomerName));
            parms.Add(new LBDbParameter("CustomerID", CustomerID, true));
            string strSQL = @"
select @CustomerID = CustomerID
from dbo.DbCustomer
where rtrim(CustomerName) = rtrim(@CustomerName)
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            CustomerID.SetValueWithObject(parms["CustomerID"].Value);
        }

        public void ReadReceiveType(FactoryArgs args, t_BigID CustomerID, out t_ID ReceiveType)
        {
            ReceiveType = new t_ID();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CustomerID", CustomerID));
            parms.Add(new LBDbParameter("ReceiveType", ReceiveType, true));
            string strSQL = @"
select @ReceiveType = ReceiveType
from dbo.DbCustomer
where CustomerID = @CustomerID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            ReceiveType.SetValueWithObject(parms["ReceiveType"].Value);
        }

        public DataTable ReadModifyDetailByCar(FactoryArgs args,t_BigID CarID,t_BigID CustomerID,t_BigID ItemID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CarID", CarID));
            parms.Add(new LBDbParameter("CustomerID", CustomerID));
            parms.Add(new LBDbParameter("ItemID", ItemID));

            string strSQL = @"
if @CustomerID > 0
begin
    select top 1 d.*
    from dbo.ModifyBillDetail d
        inner join dbo.ModifyBillHeader h on
            h.ModifyBillHeaderID = d.ModifyBillHeaderID
    where   IsApprove = 1 and
            h.CustomerID = @CustomerID and
            d.CarID = @CarID and
            d.ItemID = @ItemID
    order by h.EffectDate desc,h.ApproveTime desc
end
else
begin
    select top 1 d.*
    from dbo.ModifyBillDetail d
        inner join dbo.ModifyBillHeader h on
            h.ModifyBillHeaderID = d.ModifyBillHeaderID
    where   IsApprove = 1 and
            d.CarID = @CarID and
            d.ItemID = @ItemID
    order by h.EffectDate desc,h.ApproveTime desc
end
";
            return DBHelper.ExecuteQuery(args, strSQL, parms);
        }

        public DataTable ReadModifyDetailByItem(FactoryArgs args, t_BigID CustomerID, t_BigID ItemID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CustomerID", CustomerID));
            parms.Add(new LBDbParameter("ItemID", ItemID));

            string strSQL = @"
if @CustomerID > 0
begin
    select top 1 d.*
    from dbo.ModifyBillDetail d
        inner join dbo.ModifyBillHeader h on
            h.ModifyBillHeaderID = d.ModifyBillHeaderID
    where   IsApprove = 1 and
            h.CustomerID = @CustomerID and
            d.ItemID = @ItemID
    order by h.EffectDate desc,h.ApproveTime desc
end
else
begin
    select top 1 d.*
    from dbo.ModifyBillDetail d
        inner join dbo.ModifyBillHeader h on
            h.ModifyBillHeaderID = d.ModifyBillHeaderID
    where   IsApprove = 1 and
            d.ItemID = @ItemID
    order by h.EffectDate desc,h.ApproveTime desc
end
";
            return DBHelper.ExecuteQuery(args, strSQL, parms);
        }

        public DataTable ReadItem(FactoryArgs args, t_BigID ItemID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("ItemID", ItemID));

            string strSQL = @"
select *
from dbo.DbItemBase
where ItemID=@ItemID
";
            return DBHelper.ExecuteQuery(args, strSQL, parms);
        }

        public void UpdateInPrintCount(FactoryArgs args, t_BigID SaleCarInBillID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            string strSQL = @"
update dbo.SaleCarInBill
set PrintCount = isnull(PrintCount,0)+1
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void UpdateOutPrintCount(FactoryArgs args, t_BigID SaleCarOutBillID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarOutBillID", SaleCarOutBillID));
            string strSQL = @"
update dbo.SaleCarOutBill
set OutPrintCount = isnull(OutPrintCount,0)+1
where SaleCarOutBillID = @SaleCarOutBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void GetInsideCarCount(FactoryArgs args, out t_ID CarCount)
        {
            CarCount = new t_ID(0);
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CarCount", CarCount,true));
            string strSQL = @"
select @CarCount = sum(1)
from dbo.View_SaleCarInOutBill 
where SaleCarOutBillID is null and isnull(IsCancel,0) = 0
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            CarCount.SetValueWithObject(parms["CarCount"].Value);
        }

        public void GetTodayTotalWeight(FactoryArgs args, out t_Decimal SalesTotalWeight, out t_ID TotalCar)
        {
            DateTime dtFrom = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            DateTime dtTo = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(1);
            SalesTotalWeight = new t_Decimal(0);
            TotalCar = new t_ID(0);
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SalesTotalWeight", SalesTotalWeight, true));
            parms.Add(new LBDbParameter("TotalCar", TotalCar, true));
            parms.Add(new LBDbParameter("DTFrom", new t_DTSmall(dtFrom)));
            parms.Add(new LBDbParameter("DTTo", new t_DTSmall(dtTo)));
            string strSQL = @"
select @SalesTotalWeight = sum(SuttleWeight/1000.0), @TotalCar = count(1)
from dbo.SaleCarOutBill o
    inner join  SaleCarInBill i on
        i.SaleCarInBillID = o.SaleCarInBillID
where   i.BillStatus = 2 and 
        o.BillDate>=@DTFrom and o.BillDate<@DTTo
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            SalesTotalWeight.SetValueWithObject(parms["SalesTotalWeight"].Value);
            TotalCar.SetValueWithObject(parms["TotalCar"].Value);
        }

        public void InsertChangeBill(FactoryArgs args, out t_BigID SaleCarChangeBillID,
            t_BigID SaleCarInBillID, t_DTSmall ChangeDate, t_String ChangeBy, t_String ChangeDesc,
            t_String ChangeDetail)
        {
            SaleCarChangeBillID = new t_BigID();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarChangeBillID", SaleCarChangeBillID, true));
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("ChangeDate", ChangeDate));
            parms.Add(new LBDbParameter("ChangeBy", ChangeBy));
            parms.Add(new LBDbParameter("ChangeDesc", ChangeDesc));
            parms.Add(new LBDbParameter("ChangeDetail", ChangeDetail));

            string strSQL = @"
insert dbo.SaleCarChangeBill(SaleCarInBillID,ChangeDate,ChangeBy,ChangeDesc,ChangeDetail)
values(@SaleCarInBillID,@ChangeDate,@ChangeBy,@ChangeDesc,@ChangeDetail)

set @SaleCarChangeBillID = @@identity
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            SaleCarChangeBillID.SetValueWithObject(parms["SaleCarChangeBillID"].Value);
        }

        public DataTable GetRPReceiveBillHeader(FactoryArgs args, t_BigID SaleCarInBillID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            string strSQL = @"
select *
from dbo.RPReceiveBillHeader
where SaleCarInBillID = @SaleCarInBillID
";
            return DBHelper.ExecuteQuery(args, strSQL, parms);
        }

        public void UpdateInOutBill(FactoryArgs args, t_BigID SaleCarInBillID,t_BigID CarID, t_BigID ItemID, t_BigID CustomerID, t_Decimal Price,
            t_Decimal Amount, t_String Description,t_String ChangeDetail)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("CarID", CarID));
            parms.Add(new LBDbParameter("ItemID", ItemID));
            parms.Add(new LBDbParameter("CustomerID", CustomerID));
            parms.Add(new LBDbParameter("Price", Price));
            parms.Add(new LBDbParameter("Amount", Amount));
            parms.Add(new LBDbParameter("Description", Description));
            parms.Add(new LBDbParameter("ChangeDetail", ChangeDetail));
            parms.Add(new LBDbParameter("ChangedBy", new t_String(args.LoginName)));
            string strSQL = @"
update dbo.SaleCarInBill
set ItemID = @ItemID,
    CarID = @CarID,
    CustomerID = @CustomerID
where SaleCarInBillID = @SaleCarInBillID

update dbo.SaleCarOutBill
set Price = @Price,
    Amount = @Amount,
    Description = @Description,
    ChangedBy = @ChangedBy,
    ChangeTime = getdate(),
    ChangeDetail = @ChangeDetail
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void GetItemID(FactoryArgs args,t_String ItemName,out t_BigID ItemID)
        {
            ItemID = new t_BigID();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("ItemName", ItemName));
            parms.Add(new LBDbParameter("ItemID", ItemID,true));
            string strSQL = @"
select @ItemID=ItemID
from DbItemBase
where rtrim(ItemName) = rtrim(@ItemName)
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            ItemID.SetValueWithObject(parms["ItemID"].Value);
        }

        public void GetCustomerID(FactoryArgs args, t_String CustomerName, out t_BigID CustomerID)
        {
            CustomerID = new t_BigID();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CustomerName", CustomerName));
            parms.Add(new LBDbParameter("CustomerID", CustomerID, true));
            string strSQL = @"
select @CustomerID=CustomerID
from DbCustomer
where rtrim(CustomerName) = rtrim(@CustomerName)
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            CustomerID.SetValueWithObject(parms["CustomerID"].Value);
        }

        public void GetCarID(FactoryArgs args, t_String CarNum, out t_BigID CarID)
        {
            CarID = new t_BigID();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CarNum", CarNum));
            parms.Add(new LBDbParameter("CarID", CarID, true));
            string strSQL = @"
select @CarID=CarID
from DbCar
where rtrim(CarNum) = rtrim(@CarNum)
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            CarID.SetValueWithObject(parms["CarID"].Value);
        }

        public void GetItemName(FactoryArgs args, t_BigID ItemID, out t_String ItemName)
        {
            ItemName = new t_String();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("ItemName", ItemName, true));
            parms.Add(new LBDbParameter("ItemID", ItemID));
            string strSQL = @"
select @ItemName=ItemName
from DbItemBase
where ItemID=@ItemID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            ItemName.SetValueWithObject(parms["ItemName"].Value);
        }

        public void GetCustomerName(FactoryArgs args,  t_BigID CustomerID, out t_String CustomerName)
        {
            CustomerName = new t_String();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CustomerName", CustomerName, true));
            parms.Add(new LBDbParameter("CustomerID", CustomerID));
            string strSQL = @"
select @CustomerName=CustomerName
from DbCustomer
where CustomerID = @CustomerID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            CustomerName.SetValueWithObject(parms["CustomerName"].Value);
        }

        public void GetCarNum(FactoryArgs args, t_BigID CarID,out t_String CarNum)
        {
            CarNum = new t_String();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("CarNum", CarNum, true));
            parms.Add(new LBDbParameter("CarID", CarID));
            string strSQL = @"
select @CarNum=CarNum
from DbCar
where CarID = @CarID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
            CarNum.SetValueWithObject(parms["CarNum"].Value);
        }

        public void UpdateInBillDate(FactoryArgs args,t_BigID SaleCarInBillID,t_DTSmall BillDate,
            t_String ApproveBy,t_DTSmall ApproveTime, t_String CreateBy, t_DTSmall CreateTime, 
            t_String CancelBy, t_DTSmall CancelTime, t_DTSmall CancelByDate,t_String ImportSaleCarInBillCode)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("BillDate", BillDate));
            parms.Add(new LBDbParameter("ApproveBy", ApproveBy));
            parms.Add(new LBDbParameter("ApproveTime", ApproveTime));
            parms.Add(new LBDbParameter("CreateBy", CreateBy));
            parms.Add(new LBDbParameter("CreateTime", CreateTime));
            parms.Add(new LBDbParameter("CancelBy", CancelBy));
            parms.Add(new LBDbParameter("CancelTime", CancelTime));
            parms.Add(new LBDbParameter("CancelByDate", CancelByDate));
            parms.Add(new LBDbParameter("ImportSaleCarInBillCode", ImportSaleCarInBillCode));

            string strSQL = @"
update dbo.SaleCarInBill
set BillDate = @BillDate,
    ApproveBy = @ApproveBy,
    ApproveTime = @ApproveTime,
    CreateBy = @CreateBy,
    CreateTime = @CreateTime,
    CancelBy = @CancelBy,
    CancelTime = @CancelTime,
    CancelByDate = @CancelByDate,
    ImportSaleCarInBillCode = @ImportSaleCarInBillCode
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void UpdateOutBillDate(FactoryArgs args, t_BigID SaleCarOutBillID, t_DTSmall BillDate, 
            t_String CreateBy, t_DTSmall CreateTime,t_String ImportSaleCarOutBillCode)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarOutBillID", SaleCarOutBillID));
            parms.Add(new LBDbParameter("BillDate", BillDate));
            parms.Add(new LBDbParameter("CreateBy", CreateBy));
            parms.Add(new LBDbParameter("CreateTime", CreateTime));
            parms.Add(new LBDbParameter("ImportSaleCarOutBillCode", ImportSaleCarOutBillCode));

            string strSQL = @"
update dbo.SaleCarOutBill
set BillDate = @BillDate,
    CreateBy = @CreateBy,
    CreateTime = @CreateTime,
    ImportSaleCarOutBillCode = @ImportSaleCarOutBillCode
where SaleCarOutBillID = @SaleCarOutBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public bool VerifyIfExistsInBillCode(FactoryArgs args, t_String ImportSaleCarInBillCode)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("ImportSaleCarInBillCode", ImportSaleCarInBillCode));
            string strSQL = @"
select 1
from dbo.SaleCarInBill
where ImportSaleCarInBillCode = @ImportSaleCarInBillCode
";
            DataTable dt = DBHelper.ExecuteQuery(args, strSQL, parms);
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }

        public bool VerifyIfExistsSourceInBill(FactoryArgs args, t_BigID SaleCarInBillIDFromClient)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillIDFromClient", SaleCarInBillIDFromClient));
            string strSQL = @"
select 1
from dbo.SaleCarInBill
where SaleCarInBillIDFromClient = @SaleCarInBillIDFromClient
";
            DataTable dt = DBHelper.ExecuteQuery(args, strSQL, parms);
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }

        public bool VerifyIfExistsSourceOutBill(FactoryArgs args, t_BigID SourceSaleCarOutBillID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SourceSaleCarOutBillID", SourceSaleCarOutBillID));
            string strSQL = @"
select 1
from dbo.SaleCarOutBill
where SourceSaleCarOutBillID = @SourceSaleCarOutBillID
";
            DataTable dt = DBHelper.ExecuteQuery(args, strSQL, parms);
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }

        #region -- 采购汽油 --

        public void UpdateInBillPurchase(FactoryArgs args,t_BigID SaleCarInBillID, t_Decimal CarTare)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("CarTare", CarTare));
            parms.Add(new LBDbParameter("CreateBy", new t_String(args.LoginName)));

            string strSQL = @"
update dbo.SaleCarInBill
set CarTare = @CarTare,
    CreateBy = @CreateBy,
    BillDate = getdate(),
    CreateTime = getdate()
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void UpdateOutBillPurchase(FactoryArgs args, t_BigID SaleCarInBillID, t_Decimal SuttleWeight,
            t_Decimal Price,t_Decimal Amount,t_String SaleCarOutBillCode)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("SuttleWeight", SuttleWeight));
            parms.Add(new LBDbParameter("Price", Price));
            parms.Add(new LBDbParameter("Amount", Amount));
            parms.Add(new LBDbParameter("SaleCarOutBillCode", SaleCarOutBillCode));
            parms.Add(new LBDbParameter("CreateBy", new t_String(args.LoginName)));

            string strSQL = @"
update dbo.SaleCarOutBill
set SuttleWeight = @SuttleWeight,
    CreateBy = @CreateBy,
    BillDate = getdate(),
    CreateTime = getdate(),
    Price = @Price,
    Amount = @Amount,
    SaleCarOutBillCode = @SaleCarOutBillCode
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }
        #endregion

        public void SynchronousFinish(FactoryArgs args, t_BigID SaleCarInBillID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));

            string strSQL = @"
update dbo.SaleCarInBill
set IsSynchronousToServer = 1,
    SynchronousToServerTime=getdate()
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void SynchronousK3OutBillStatus(FactoryArgs args, t_BigID SaleCarInBillID,
            t_Bool IsSynchronousToK3OutBill,t_String SynchronousK3OutBillResult)
        {
            IsSynchronousToK3OutBill.IsNullToZero();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("IsSynchronousToK3OutBill", IsSynchronousToK3OutBill));
            parms.Add(new LBDbParameter("SynchronousK3OutBillResult", SynchronousK3OutBillResult));
            parms.Add(new LBDbParameter("SynchronousK3ByOutBill", new t_String(args.LoginName)));

            string strSQL = @"
update dbo.SaleCarInBill
set IsSynchronousToK3OutBill = @IsSynchronousToK3OutBill,
    SynchronousToTimeOutBill = getdate(),
    SynchronousK3ByOutBill= @SynchronousK3ByOutBill,
    SynchronousK3OutBillResult = @SynchronousK3OutBillResult
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void SynchronousK3ReceiveStatus(FactoryArgs args, t_BigID SaleCarInBillID,
            t_Bool IsSynchronousToK3Receive, t_String SynchronousK3ReceiveResult)
        {
            IsSynchronousToK3Receive.IsNullToZero();
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("IsSynchronousToK3Receive", IsSynchronousToK3Receive));
            parms.Add(new LBDbParameter("SynchronousK3ReceiveResult", SynchronousK3ReceiveResult));
            parms.Add(new LBDbParameter("SynchronousK3ByReceive", new t_String(args.LoginName)));

            string strSQL = @"
update dbo.SaleCarInBill
set IsSynchronousToK3Receive = @IsSynchronousToK3Receive,
    SynchronousToTimeReceive = getdate(),
    SynchronousK3ByReceive= @SynchronousK3ByReceive,
    SynchronousK3ReceiveResult = @SynchronousK3ReceiveResult
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void SynchronousK3Error(FactoryArgs args, t_BigID SaleCarInBillID,t_String SynchronousK3Error)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));
            parms.Add(new LBDbParameter("SynchronousK3Error", SynchronousK3Error));

            string strSQL = @"
update dbo.SaleCarInBill
set SynchronousK3Error= @SynchronousK3Error
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void GetSalesBillInOut(FactoryArgs args,t_String BillIDStr,out DataTable dtInBill,out DataTable dtOutBill)
        {
            dtInBill = new DataTable();
            dtOutBill = new DataTable();
            string strSQL = string.Format(@"
select *
from dbo.SaleCarInBill
where SaleCarInBillID in ({0})
", BillIDStr.Value);
            dtInBill = DBHelper.ExecuteQuery(args, strSQL);

            strSQL = string.Format(@"
select *
from dbo.SaleCarOutBill
where SaleCarInBillID in ({0})
", BillIDStr.Value);
            dtOutBill = DBHelper.ExecuteQuery(args, strSQL);
        }

        public void RemoveInOutBill(FactoryArgs args, t_BigID SaleCarInBillID)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleCarInBillID", SaleCarInBillID));

            string strSQL = @"
delete dbo.SaleCarOutBill
where SaleCarInBillID = @SaleCarInBillID

delete dbo.SaleCarInBill
where SaleCarInBillID = @SaleCarInBillID
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }

        public void SaleCarBillRemoved_Insert(FactoryArgs args,t_nText SaleInBillRemoveJson,t_nText SaleOutBillRemoveJson)
        {
            LBDbParameterCollection parms = new LBDbParameterCollection();
            parms.Add(new LBDbParameter("SaleInBillRemoveJson", SaleInBillRemoveJson));
            parms.Add(new LBDbParameter("SaleOutBillRemoveJson", SaleOutBillRemoveJson));
            parms.Add(new LBDbParameter("RemovedBy", new t_String(args.LoginName)));

            string strSQL = @"
insert SaleCarBillRemoved(SaleInBillRemoveJson,SaleOutBillRemoveJson,RemovedTime,RemovedBy)
values(@SaleInBillRemoveJson,@SaleOutBillRemoveJson,getdate(),@RemovedBy)
";
            DBHelper.ExecuteNonQuery(args, System.Data.CommandType.Text, strSQL, parms, false);
        }
    }
}
