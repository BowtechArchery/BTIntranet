using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.SupplyChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.BusinessLogic.SupplyChain
{
    public static class OpenOrderProcessor
    {
        public static List<OpenOrderModel> LoadOpenOrders()
        {
            var p = new Dapper.DynamicParameters();

            string sql = @"SELECT dtl.sodtl_order_no AS SalesOrderNumber, 
		                    dtl.sodtl_line_no AS SalesOrderLineNumber, 
		                    dtl.sodtl_item_code AS ItemCode,
		                    loi_itemshortdesc As ItemDescription, 
		                    cus.clo_cust_name CustomerName, 
		                    hdr.sohdr_order_from_cust AS CustomerNumber, 
		                    cou_status AS DealerStatus, 
		                    hdr.sohdr_order_status AS OrderStatus, 
		                    dtl.sodtl_orderpull_allocated_qty AS AllocatedQuantity, 
		                    hdr.sohdr_order_priority AS Priority, 
		                    dtl.sodtl_rem_qty AS OpenQuantity, 
		                    CONVERT(DATE,dtl.sodtl_pricing_date) AS OrderDate,
		                    CONVERT(DATE,dtl.sodtl_req_date_dflt) AS RequiredDate, 
		                    CONVERT(DATE,dtl.sodtl_prm_date_dflt) AS PromiseDate,
		                    CONVERT(DATE,dtl.sodtl_modified_date) AS ModifiedDate, 
		                    CONVERT(DATE,soc.dont_ship_defore_date) AS DNSBefore,
		                    tns_notes AS Comments,
		                    ptwh.woh_wo_no AS WorkOrderNumber,
		                    pepperiorderdetail.CustomBowModel,
		                    pepperiorderdetail.CustomBowHand,
		                    pepperiorderdetail.CustomBowDrawWeight,
		                    pepperiorderdetail.CustomBowRiserColor,
		                    pepperiorderdetail.CustomBowLimbColor
	                    FROM so_order_item_dtl dtl LEFT JOIN so_order_hdr hdr ON dtl.sodtl_order_no=hdr.sohdr_order_no
		                    LEFT JOIN itm_loi_itemhdr ON dtl.sodtl_item_code=loi_itemcode
		                    LEFT JOIN not_tns_transaction_notes ON dtl.sodtl_order_no=tns_document_no 
			                    AND tns_notes_for_id='SALE ORDER' 
			                    AND tns_notes_level=1 
			                    AND tns_line_no=dtl.sodtl_line_no
		                    LEFT JOIN cust_lo_info cus ON hdr.sohdr_order_from_cust=cus.clo_cust_code
		                    LEFT JOIN cust_ou_info cou ON hdr.sohdr_order_from_cust=cou_cust_code
		                    LEFT JOIN cust_bu_info cbu ON hdr.sohdr_order_from_cust=cbu.cbu_cust_code
		                    LEFT JOIN SAV_MINI_BOM_UDS ON dtl.sodtl_item_code=Item_code
		                    LEFT JOIN itm_iou_itemvarhdr ON dtl.sodtl_item_code=iou_itemcode
		                    LEFT JOIN SAV_ORDER_CANCEL soc ON dtl.sodtl_order_no = soc.order_no
		                    LEFT JOIN pmd_twoh_wo_headr ptwh ON dtl.sodtl_order_no = ptwh.woh_so_no 
			                    AND dtl.sodtl_item_code = ptwh.woh_item_no
		                    OUTER APPLY ( SELECT TOP 1 
						                    pod.CustomBowModel,
						                    pod.CustomBowHand,
						                    pod.CustomBowDrawWeight,
						                    pod.CustomBowRiserColor,
						                    pod.CustomBowLimbColor
						                    FROM [RVWDB].DataWarehouse.dbo.Pepperi_OrderDetail pod
						                    WHERE pod.PepperiOrderNum = dbo.fn_StripChars(dtl.sodtl_order_no, '^--=0-9') 
						                    AND	pod.ItemCode = dtl.sodtl_item_code
					                    ) pepperiorderdetail
	                    WHERE dtl.sodtl_line_status <>'DL' 
	                    AND hdr.sohdr_order_status <>'DL' 
	                    AND dtl.sodtl_rem_qty > 0 
	                    AND dtl.sodtl_item_code LIKE '%%' 
	                    AND UPPER(hdr.sohdr_order_from_cust) LIKE '%%' 
	                    AND ISNULL(hdr.sohdr_cust_po_no,'') LIKE '%%' 
	                    AND UPPER(loi_itemshortdesc) LIKE '%%' 
	                    AND UPPER(dtl.sodtl_item_code) LIKE '%%' 
	                    AND ISNULL(hdr.sohdr_sales_channel,'') LIKE '%%' 
	                    AND CONVERT(date,dtl.sodtl_sch_date_dflt) BETWEEN CONVERT(date,'01/01/1900') 
	                    AND CONVERT(date,'12/31/9999')  
	                    AND UPPER(hdr.sohdr_order_status) LIKE '%%' 
	                    AND UPPER(hdr.sohdr_order_priority) IN ('1','2','3','4','5','6','7','8','9') 
	                    AND cbu_market LIKE '%%' 
	                    AND UPPER(clo_cust_name) LIKE '%%' 
	                    AND dtl.sodtl_order_no LIKE '%%' 
	                    AND hdr.sohdr_pay_term_code LIKE '%%' 
	                    ORDER BY dtl.sodtl_sch_date_dflt;";

            return SQLDataAccess.LoadDataSCMDB<OpenOrderModel>(sql, p);
        }
    }
}
